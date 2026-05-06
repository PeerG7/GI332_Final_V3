using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text; // สำหรับ Encoding

public class UiMainMenu : MonoBehaviour
{
    public GameObject Option, Menu, Play, Host, Client;
    [Header("Relay UI")]
    public TMP_InputField joinCodeInput;
    [Header("Spawn Settings")]
    public Vector3 hostSpawnPos = new Vector3(-3.28f, 0.55f, 10.63f);
    public Vector3 clientSpawnPos = new Vector3(0f, 0.55f, 0f);
    public GameObject[] playerPrefabs;
    // เพิ่มส่วนนี้เพื่อเก็บ ID ของตัวละครที่เลือก
    private int selectedCharacterIndex = 0;

    public static string JoinCode;
    public void Awake() => ShowPanel(Menu);

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
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

            // ก่อน StartHost ให้ใส่ข้อมูลตัวละครของ Host เองลงไปใน ConnectionData ด้วย
            string payload = selectedCharacterIndex.ToString();
            byte[] payloadBytes = Encoding.ASCII.GetBytes(payload);
            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
        }
        catch (System.Exception e) { Debug.LogError(e); }
    }
    public void Connect()
    {
        // ต้องเอาค่า selectedIndex ที่ได้จากการกดปุ่มมาแปลงเป็น Byte
        byte[] payload = System.Text.Encoding.ASCII.GetBytes(selectedCharacterIndex.ToString());

        // ส่ง Payload นี้เข้าไปใน NetworkConfig
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payload;

        // แล้วค่อยสั่ง StartClient
        NetworkManager.Singleton.StartClient();
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

            // ส่ง ID ตัวละครที่ Client เลือกไปให้ Server
            string payload = selectedCharacterIndex.ToString();
            byte[] payloadBytes = Encoding.ASCII.GetBytes(payload);
            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

            NetworkManager.Singleton.StartClient();
        }
        catch (System.Exception e) { Debug.LogError($"Relay Client Error: {e.Message}"); }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = true;

        // อ่านค่าจาก Payload
        string payload = System.Text.Encoding.ASCII.GetString(request.Payload);

        // ตรวจสอบว่าแปลงเป็นตัวเลขสำเร็จไหม
        if (int.TryParse(payload, out int charIndex))
        {
            // ตรวจสอบ Index จาก Array playerPrefabs ที่เราสร้างไว้ตอนต้น
            if (playerPrefabs != null && charIndex >= 0 && charIndex < playerPrefabs.Length)
            {
                // ดึงคอมโพเนนต์ NetworkObject จาก Prefab
                var networkObject = playerPrefabs[charIndex].GetComponent<NetworkObject>();

                // ใน Netcode รุ่นใหม่/บางรุ่น จะใช้ชื่อ PrefabIdHash หรือ GlobalObjectIdHash
                // หากอันเดิมแดง ให้เปลี่ยนมาใช้คำสั่งนี้ ซึ่งเป็นวิธีสากลครับ
                response.PlayerPrefabHash = networkObject.PrefabIdHash;
            }
        }

        // ตั้งค่าตำแหน่งเกิด
        response.Position = (request.ClientNetworkId == NetworkManager.Singleton.LocalClientId) ? hostSpawnPos : clientSpawnPos;
        response.Rotation = Quaternion.identity;
        response.Pending = false;
    }
    // ฟังก์ชันสำหรับปุ่มเลือกตัวละคร (Jumper, Tank, Dasher)
    public void SelectCharacter(int index)
    {
        selectedCharacterIndex = index; // บรรทัดนี้สำคัญมาก! ต้องมีเพื่อเปลี่ยนค่าจาก 0 เป็น 1 หรือ 2
        Debug.Log("Selected Character Index: " + selectedCharacterIndex);
    }

    // --- UI Buttons ---
    public void ClientButton() => ShowPanel(Client);
    public void HostButton() => ShowPanel(Host);
    public void Options() => ShowPanel(Option);
    public void MainMenuButton() => ShowPanel(Menu);
    public void PlayButton() => ShowPanel(Play);
    public void QuitGame() => Application.Quit();
}