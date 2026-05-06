using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text;

public class UiMainMenu : MonoBehaviour
{
    public GameObject Option, Menu, Play, Host, Client;
    [Header("Relay UI")]
    public TMP_InputField joinCodeInput;
    [Header("Spawn Settings")]
    public Vector3 hostSpawnPos;
    public Vector3 clientSpawnPos;
    public GameObject[] playerPrefabs; // 0=Jumper, 1=Tank, 2=Dasher

    private int selectedCharacterIndex = 0;
    public static string JoinCode;

    // ✅ เก็บ index แยก Host / Client ไว้ใน static เพื่อให้ ApprovalCheck อ่านได้
    public static int HostCharacterIndex = 0;
    public static int ClientCharacterIndex = 0;

    public void Awake() => ShowPanel(Menu);

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (System.Exception e) { Debug.LogError(e); }

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
    }

    private void ShowPanel(GameObject panelToShow)
    {
        Menu.SetActive(panelToShow == Menu);
        Client.SetActive(panelToShow == Client);
        Host.SetActive(panelToShow == Host);
        Option.SetActive(panelToShow == Option);
        Play.SetActive(panelToShow == Play);
    }

    public async void StartRelayHost()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetHostRelayData(
                allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData
            );

            // ✅ บันทึก index ของ Host ไว้ใน static
            HostCharacterIndex = selectedCharacterIndex;

            // ✅ ส่ง payload เป็น index ของ Host ไปด้วย (ใช้ใน ApprovalCheck)
            string payload = selectedCharacterIndex.ToString();
            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(payload);

            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
        }
        catch (System.Exception e) { Debug.LogError(e); }
    }

    public async void StartRelayClient()
    {
        try
        {
            string code = joinCodeInput.text;
            if (string.IsNullOrEmpty(code)) return;

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetClientRelayData(
                joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes, joinAllocation.Key,
                joinAllocation.ConnectionData, joinAllocation.HostConnectionData
            );

            // ✅ ส่ง index ตัวละครของ Client ไปให้ Server ผ่าน ConnectionData
            string payload = selectedCharacterIndex.ToString();
            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(payload);

            NetworkManager.Singleton.StartClient();
        }
        catch (System.Exception e) { Debug.LogError($"Relay Client Error: {e.Message}"); }
    }

    private void ApprovalCheck(
    NetworkManager.ConnectionApprovalRequest request,
    NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = true;

        int charIndex = 0;

        // ✅ ป้องกัน Payload ว่าง (กรณี Host)
        if (request.Payload != null && request.Payload.Length > 0)
        {
            string payload = Encoding.ASCII.GetString(request.Payload);
            int.TryParse(payload, out charIndex);
        }

        Debug.Log($"[Approval] ClientId={request.ClientNetworkId} | CharIndex={charIndex}");

        // ✅ Set Prefab
        if (playerPrefabs != null && charIndex >= 0 && charIndex < playerPrefabs.Length)
        {
            var networkObject = playerPrefabs[charIndex].GetComponent<NetworkObject>();
            if (networkObject != null)
                response.PlayerPrefabHash = networkObject.PrefabIdHash;
        }

        // ✅ แก้การเช็ค Host/Client
        bool isHost = (request.ClientNetworkId == 0);
        response.Position = isHost ? hostSpawnPos : clientSpawnPos;
        response.Rotation = Quaternion.identity;
        response.Pending = false;
    }

    // ✅ ปุ่มเลือกตัวละคร
    public void SelectCharacter(int index)
    {
        selectedCharacterIndex = index;
        Debug.Log("Selected Character: " + index);
    }

    // --- UI Buttons ---
    public void ClientButton() => ShowPanel(Client);
    public void HostButton() => ShowPanel(Host);
    public void Options() => ShowPanel(Option);
    public void MainMenuButton() => ShowPanel(Menu);
    public void PlayButton() => ShowPanel(Play);
    public void QuitGame() => Application.Quit();
}