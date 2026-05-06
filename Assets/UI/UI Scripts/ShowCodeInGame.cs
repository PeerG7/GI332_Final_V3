using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ShowCodeInGame : NetworkBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI codeDisplayPrefab; // ลาก Text ที่จะโชว์รหัสมาใส่
    public GameObject hostOnlyControls;
    public GameObject InGameUi; // ลาก Parent ของปุ่มที่มีเฉพาะ Host มาใส่ (ถ้ามี)
    public GameObject LobbyUi;
    public void Awake()
    {
        hostOnlyControls.SetActive(true);
        codeDisplayPrefab.gameObject.SetActive(true);
        InGameUi.SetActive(false);
    }
    public override void OnNetworkSpawn()
    {
        if (hostOnlyControls != null)
        {
            if (codeDisplayPrefab != null)
            {
                if (!string.IsNullOrEmpty(UiMainMenu.JoinCode))
                {
                    codeDisplayPrefab.text = "Room Code: " + UiMainMenu.JoinCode;
                }
            }
            hostOnlyControls.SetActive(IsHost);
            codeDisplayPrefab.gameObject.SetActive(IsHost);
        }
    }
    public void CopyJoinCode()
    {
        string code = UiMainMenu.JoinCode;
        if (!string.IsNullOrEmpty(code))
        {
            GUIUtility.systemCopyBuffer = code;
            Debug.Log("Copied Code: " + code);
        }
    }
    public void StartGame()
    {
        // 1. เช็คว่าคนกดต้องเป็น Host/Server เท่านั้น
        if (!IsServer) return;

        // 2. เรียก ClientRpc เพื่อให้คำสั่งนี้ทำงานในทุกเครื่อง (รวมถึง Host ด้วย)
        StartGameClientRpc();
    }
    [ClientRpc]
    private void StartGameClientRpc()
    {
        // โค้ดส่วนนี้จะทำงานใน "ทุกเครื่อง" ที่เชื่อมต่ออยู่
        if (hostOnlyControls != null) hostOnlyControls.SetActive(false);
        if (codeDisplayPrefab != null) codeDisplayPrefab.gameObject.SetActive(false);

        if (InGameUi != null) InGameUi.SetActive(true);
        if (InGameUi != null) LobbyUi.SetActive(false);
        Debug.Log("Game Started: UI updated for everyone!");
    }
}
