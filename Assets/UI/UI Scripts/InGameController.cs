using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameController : NetworkBehaviour
{
    public static InGameController Instance;

    [Header("Spawn Points")]
    public Transform spawnPointHost;
    public Transform spawnPointClient;

    [Header("UI Sliders")]
    public Slider hpSliderHost;
    public Slider cdSliderHost;
    public Slider hpSliderClient;
    public Slider cdSliderClient;

    [Header("Round System")]
    public NetworkVariable<int> hostScore = new NetworkVariable<int>(0);
    public NetworkVariable<int> clientScore = new NetworkVariable<int>(0);
    public int maxWins = 2;

    [Header("Score UI")]
    public TextMeshProUGUI scoreText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    /*public void StartGame()
    {
        if (!IsServer) return;

        StartGameClientRpc(); // แก้ UI
        InGameController.Instance.RequestStartServerRpc(); // ✅ เพิ่มบรรทัดนี้
    }*/
    void Update()
    {
        if (!IsClient && !IsServer) return;

        if (scoreText != null)
        {
            scoreText.text = $" {hostScore.Value} - {clientScore.Value}";
        }

        // Guard: NetworkManager อาจ null ระหว่าง Shutdown
        if (NetworkManager.Singleton == null) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null) continue;

            Player playerScript = client.PlayerObject.GetComponent<Player>();
            if (playerScript != null)
            {
                bool isHost = client.ClientId == 0;
                UpdatePlayerDisplay(isHost, playerScript.Hp.Value, playerScript.Cooldown.Value);
            }
        }
    }

    void UpdatePlayerDisplay(bool isHost, int hp, float cd)
    {
        if (isHost)
        {
            if (hpSliderHost) hpSliderHost.value = hp;
            if (cdSliderHost) cdSliderHost.value = cd;
        }
        else
        {
            if (hpSliderClient) hpSliderClient.value = hp;
            if (cdSliderClient) cdSliderClient.value = cd;
        }
    }

    public void BackToMenuButton()
    {
        if (IsServer)
            BackToMenuClientRpc();
        else
            LeaveGame();
    }

    public void OnPlayerDie(ulong deadClientId)
    {
        if (!IsServer) return;

        // Guard: NetworkManager อาจ null
        if (NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(deadClientId, out var client))
        {
            if (client.PlayerObject != null)
            {
                var player = client.PlayerObject.GetComponent<Player>();
                if (player != null)
                {
                    player.HidePlayerClientRpc();
                }
            }
        }

        if (deadClientId == 0)
            clientScore.Value++;
        else
            hostScore.Value++;

        bool isHostMatchWinner = hostScore.Value >= maxWins;
        bool isClientMatchWinner = clientScore.Value >= maxWins;

        if (isHostMatchWinner || isClientMatchWinner)
        {
            GoToResultScenesClientRpc(isHostMatchWinner);
        }
        else
        {
            bool hostWonThisRound = (deadClientId != 0);

            // Guard: GameUIManager อาจ null
            if (GameUIManager.Instance != null)
                GameUIManager.Instance.ShowRoundWinnerClientRpc(hostWonThisRound);
            else
                Debug.LogWarning("[InGameController] GameUIManager.Instance is null");
        }
    }

    [ClientRpc]
    private void GoToResultScenesClientRpc(bool hostWinsMatch)
    {
        ShutdownNetwork();

        if (IsHost)
            SceneManager.LoadScene(hostWinsMatch ? "!WinScene" : "!LoseScene");
        else
            SceneManager.LoadScene(hostWinsMatch ? "!LoseScene" : "!WinScene");
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestNextRoundServerRpc()
    {
        if (NetworkManager.Singleton == null) return;

        ExecuteTeleport();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null) continue;
            var p = client.PlayerObject.GetComponent<Player>();
            if (p != null) p.ResetPlayerStatus();
        }

        if (GameUIManager.Instance != null)
            GameUIManager.Instance.ResetRoundUiClientRpc();
        else
            Debug.LogWarning("[InGameController] GameUIManager.Instance is null");
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestStartServerRpc()
    {
        if (NetworkManager.Singleton == null) return;

        ExecuteTeleport();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null) continue;
            var p = client.PlayerObject.GetComponent<Player>();
            if (p != null)
            {
                p.ResetPlayerStatus();
                p.StartGame();
            }
        }

        if (GameUIManager.Instance != null)
            GameUIManager.Instance.ResetRoundUiClientRpc();
        else
            Debug.LogWarning("[InGameController] GameUIManager.Instance is null");
    }

    public void ExecuteTeleport()
    {
        if (!IsServer) return;
        if (NetworkManager.Singleton == null) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObject = client.PlayerObject;
            if (playerObject == null) continue;

            Transform spawnPoint = (client.ClientId == 0) ? spawnPointHost : spawnPointClient;
            if (spawnPoint == null)
            {
                Debug.LogWarning($"[InGameController] SpawnPoint for ClientId {client.ClientId} is null!");
                continue;
            }

            MovePlayerClientRpc(playerObject.NetworkObjectId, spawnPoint.position);
        }
    }

    [ClientRpc]
    private void MovePlayerClientRpc(ulong networkObjectId, Vector3 targetPosition)
    {
        if (NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var targetNetObj))
        {
            var cc = targetNetObj.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            targetNetObj.transform.position = targetPosition;
            if (cc != null) cc.enabled = true;
        }
    }

    [ClientRpc]
    private void BackToMenuClientRpc() => LeaveGame();

    private void LeaveGame()
    {
        ShutdownNetwork();
        SceneManager.LoadScene("MainMenu_New");
    }

    // Helper รวม Shutdown logic ไว้ที่เดียว ป้องกัน null และ double-shutdown
    private void ShutdownNetwork()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}