using UnityEngine;
//using TMPro;
//using Unity.Netcode;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mmPanel;      // หน้า Main Menu (Play, Op, Ex)
    public GameObject playPanel;    // หน้าเลือก Host/Client
    public GameObject hostPanel;    // หน้าตั้งค่า Host
    public GameObject clientPanel;  // หน้าตั้งค่า Client (ใส่ Code)
    public GameObject optionsPanel; // หน้าตั้งค่าเสียง
    public GameObject lobbyPanel;   // หน้า Lobby ก่อนเริ่มเกม
    //public TextMeshProUGUI codeText; // ลาก Text ในหน้า Lobby ที่จะแสดงรหัสมาใส่

    private void Start()
    {
        // เริ่มต้นให้เปิดเฉพาะหน้า Main Menu
        OpenPanel(mmPanel);
    }

    public void OpenPanel(GameObject panelToOpen)
    {
        // ตรวจสอบก่อนว่าตัวแปรเหล่านี้ถูก Assign หรือยัง (ป้องกัน Error)
        if (mmPanel != null) mmPanel.SetActive(false);
        if (playPanel != null) playPanel.SetActive(false);
        if (hostPanel != null) hostPanel.SetActive(false);
        if (clientPanel != null) clientPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);

        // เปิด Panel ที่ส่งมา
        if (panelToOpen != null)
        {
            panelToOpen.SetActive(true);
        }
        else
        {
            Debug.LogWarning("คุณพยายามเปิด Panel ที่ว่างเปล่า (Null) กรุณาตรวจสอบการตั้งค่าที่ปุ่มครับ");
        }
    }
    //private void OnEnable()
    //{
    //    // เมื่อหน้า Lobby ถูกเปิดขึ้นมา ให้ดึงรหัสมาโชว์
    //    DisplayCode();
    //}

    //public void DisplayCode()
    //{
    //    if (codeText == null) return;

    //    // ในกรณีที่ใช้ Unity Relay (แนะนำสำหรับการเล่นข้ามอินเทอร์เน็ต)
    //    // คุณต้องเก็บค่า Code ไว้ตอน StartHost แล้วนำมาแสดงที่นี่
    //    // แต่ถ้าเป็นการเล่นแบบ LAN/IP ปกติ มักจะโชว์เลข IP หรือเลข Port แทน

    //    // ตัวอย่างถ้าคุณมีการเก็บรหัสไว้ในตัวแปร Static หรือ Singleton
    //    // codeText.text = "Code: " + YourRelayManager.Instance.JoinCode;

    //    codeText.text = "Room Code: 123456"; // สำหรับทดสอบ UI
    //}
}