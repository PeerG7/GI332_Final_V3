using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class MultiplayerUI : MonoBehaviour
{
    public UIManager uiManager;
    public TMP_InputField joinCodeInput;
    public TMP_Text lobbyCodeDisplay;

    public async void StartHost()
    {
        // 1. ถ้ามีระบบ Relay ให้ขอ Code ตรงนี้
        // string relayCode = await RelayManager.Instance.CreateRelay();

        if (NetworkManager.Singleton.StartHost())
        {
            // แสดง Code จำลอง (หรือจริงจาก Relay) ในหน้า Lobby
            if (lobbyCodeDisplay != null) lobbyCodeDisplay.text = "Code: " + Random.Range(1000, 9999);

            // เปลี่ยนหน้า UI ไปยัง Lobby Panel (ยังไม่เปลี่ยน Scene)
            uiManager.OpenPanel(uiManager.lobbyPanel);
        }
    }

    public async void StartClient()
    {
        string code = joinCodeInput.text;
        // if (await RelayManager.Instance.JoinRelay(code)) { ... }

        if (NetworkManager.Singleton.StartClient())
        {
            if (lobbyCodeDisplay != null) lobbyCodeDisplay.text = "Joined: " + code;
            uiManager.OpenPanel(uiManager.lobbyPanel);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}