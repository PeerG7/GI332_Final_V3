using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetworkSceneLoader : NetworkBehaviour
{
    // เรียกฟังก์ชันนี้จากปุ่ม Play ในหน้า Lobby (เฉพาะ Host)
    public void LoadGame()
    {
        if (IsServer)
        {
            // เปลี่ยนจาก Lobby ไปยังฉากเล่นเกมจริง
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);   //ชื่อ Scene เกมอยู่ตรงนี้นะเปา และก็บรรทัดที่ 32 ด้วย
        }
    }

    private void Start()
    {
        // ตรวจสอบทุกครั้งที่มีการเปลี่ยน Scene สำเร็จ
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
        }
    }

    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        // เมื่อโหลดฉากหลักสำเร็จ (LoadEventCompleted)
        if (sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted)
        {
            // ถ้าฉากที่โหลดเสร็จคือฉากเล่นเกม ให้โหลด UI ซ้อนทับเข้าไป
            if (sceneEvent.SceneName == "GameScene") //ชื่อ Scene เกมอยู่ตรงนี้นะเปา และก็บรรทัดที่ 11 ด้วย
            {
                // โหลด UIScene แบบ Additive (ซ้อนทับ)
                SceneManager.LoadScene("UIScene", LoadSceneMode.Additive);
            }
        }
    }

    public override void OnDestroy()
    {
        // ป้องกัน Error เมื่อลบ Object นี้
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
        }
        base.OnDestroy();
    }
}