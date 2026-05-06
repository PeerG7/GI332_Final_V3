using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameController : NetworkBehaviour
{
    public static InGameController Instance; // ทำ Singleton ให้เรียกง่าย

    [Header("Spawn Points")]
    public Transform spawnPointHost;
    public Transform spawnPointClient;

    [Header("UI Sliders")]
    public Slider hpSliderHost;
    public Slider cdSliderHost;
    public Slider hpSliderClient;
    public Slider cdSliderClient;

    [Header("Round System")]
    // ใช้ NetworkVariable เพื่อให้แต้มตรงกันทุกเครื่อง
    public NetworkVariable<int> hostScore = new NetworkVariable<int>(0);
    public NetworkVariable<int> clientScore = new NetworkVariable<int>(0);
    public int maxWins = 2; // ชนะ 2 ใน 3
    [Header("Score UI")]
    public TextMeshProUGUI scoreText;
    
    private void Awake()
    {
        // --- 2. ต้องกำหนดค่าให้ Instance ตอนเริ่มเกม ---
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (!IsClient && !IsServer) return;
        if (scoreText != null)
        {
            // ดึงค่า .Value จาก NetworkVariable มาแสดง
            scoreText.text = $" {hostScore.Value} - {clientScore.Value}";
        }
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                Player playerScript = client.PlayerObject.GetComponent<Player>();
                if (playerScript != null)
                {
                    bool isHost = client.ClientId == 0;
                    UpdatePlayerDisplay(isHost, playerScript.Hp.Value, playerScript.Cooldown.Value);
                }
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
        if (IsServer) BackToMenuClientRpc();
        else LeaveGame();
    }
    // ฟังก์ชันตัดสินเมื่อมีคนตาย (เรียกจาก Player.cs บน Server)
    public void OnPlayerDie(ulong deadClientId)
    {
        if (!IsServer) return;
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(deadClientId, out var client))
        {
            var player = client.PlayerObject.GetComponent<Player>();
            if (player != null)
            {
                player.HidePlayerClientRpc(); // สั่งให้หายไปทุกเครื่อง
            }
        }
        if (deadClientId == 0)
        {
            clientScore.Value++;

        }else 
        {
            hostScore.Value++;
        } 

        // เช็คว่ามีใครชนะครบ 2 หรือยัง
        bool isHostMatchWinner = hostScore.Value >= maxWins;
        bool isClientMatchWinner = clientScore.Value >= maxWins;

        if (isHostMatchWinner || isClientMatchWinner)
        {
            // ถ้าชนะครบ 2/3 แล้ว ให้ไป Scene ผลแพ้ชนะเลย
            GoToResultScenesClientRpc(isHostMatchWinner);
        }
        else
        {
            // ถ้ายังไม่ครบ ให้โชว์หน้าจอชนะรอบนั้น (isP1Winner คือ true ถ้า Host ได้แต้ม)
            bool hostWonThisRound = (deadClientId != 0);
            GameUIManager.Instance.ShowRoundWinnerClientRpc(hostWonThisRound);
        }
    }
    [ClientRpc]
    private void GoToResultScenesClientRpc(bool hostWinsMatch)
    {
        NetworkManager.Singleton.Shutdown();

        if (IsHost)
        {
            SceneManager.LoadScene(hostWinsMatch ? "!WinScene" : "!LoseScene");
        }
        else
        {
            SceneManager.LoadScene(hostWinsMatch ? "!LoseScene" : "!WinScene");
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestNextRoundServerRpc()
    {
        // 1. วาร์ปผู้เล่นกลับจุดเกิด
        ExecuteTeleport();
        // 2. รีเซ็ตเลือดทุกคน
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var p = client.PlayerObject.GetComponent<Player>();
            if (p != null)
            {
                // เรียกฟังก์ชันรีเซ็ตที่เราสร้างไว้ใน Player.cs
                p.ResetPlayerStatus();
            }
        }
        // 3. สั่งทุกเครื่องปิดหน้าจอชนะรอบ แล้วกลับไปหน้า Gameplay
        GameUIManager.Instance.ResetRoundUiClientRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestStartServerRpc()
    {

        // 1. วาร์ปผู้เล่นกลับจุดเกิด
        ExecuteTeleport();
        
        // 2. รีเซ็ตเลือดทุกคน
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var p = client.PlayerObject.GetComponent<Player>();
            if (p != null)
            {
                // เรียกฟังก์ชันรีเซ็ตที่เราสร้างไว้ใน Player.cs
                p.ResetPlayerStatus();
                p.StartGame();
            }
        }
        // 3. สั่งทุกเครื่องปิดหน้าจอชนะรอบ แล้วกลับไปหน้า Gameplay
        GameUIManager.Instance.ResetRoundUiClientRpc();
    }

    // ฟังก์ชันวาร์ปที่มีอยู่แล้ว
    public void ExecuteTeleport()
    {
        if (!IsServer) return;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObject = client.PlayerObject;
            if (playerObject != null)
            {
                Vector3 targetPos = (client.ClientId == 0) ? spawnPointHost.position : spawnPointClient.position;
                MovePlayerClientRpc(playerObject.NetworkObjectId, targetPos);
            }
        }
    }
    [ClientRpc]
    private void MovePlayerClientRpc(ulong networkObjectId, Vector3 targetPosition)
    {
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
        NetworkManager.Singleton.Shutdown();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu_New"); // เปลี่ยนเป็นชื่อ Scene เมนูของคุณ
    }
}