using Unity.Netcode;
using UnityEngine;

public class GameUIManager : NetworkBehaviour
{
    public static GameUIManager Instance;

    [Header("UI Panels")]
    public GameObject lobbyUi;
    public GameObject gameplayUi;
    public GameObject p1WinUi; // หน้าจอแสดงเมื่อ Host ชนะในรอบนั้น
    public GameObject p2WinUi; // หน้าจอแสดงเมื่อ Client ชนะในรอบนั้น

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void HideAllUi()
    {
        if (lobbyUi) lobbyUi.SetActive(false);
        if (gameplayUi) gameplayUi.SetActive(false);
        if (p1WinUi) p1WinUi.SetActive(false);
        if (p2WinUi) p2WinUi.SetActive(false);
    }

    // เรียกเมื่อมีคนชนะในรอบนั้นๆ
    [ClientRpc]
    public void ShowRoundWinnerClientRpc(bool isP1Winner)
    {
        HideAllUi();
        if (isP1Winner)
        {
            if (p1WinUi) p1WinUi.SetActive(true);
        }
        else
        {
            if (p2WinUi) p2WinUi.SetActive(true);
        }
    }

    // ฟังก์ชันสำหรับปุ่ม "เริ่ม Round ใหม่" (ใส่ไว้ในปุ่มของทั้ง p1WinUi และ p2WinUi)
    public void OnNextRoundButtonClick()
    {
        if (InGameController.Instance != null)
        {
            // ส่งคำสั่งไปที่ Server เพื่อเริ่มรอบใหม่
            InGameController.Instance.RequestNextRoundServerRpc();
        }
    }

    [ClientRpc]
    public void ResetRoundUiClientRpc()
    {
        HideAllUi();
        if (gameplayUi) gameplayUi.SetActive(true);
    }

    public void OnBackToMenuClick()
    {
        if (InGameController.Instance != null)
        {
            InGameController.Instance.BackToMenuButton();
        }
    }
}