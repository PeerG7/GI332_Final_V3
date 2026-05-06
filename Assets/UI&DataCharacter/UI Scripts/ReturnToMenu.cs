using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class ReturnToMenu : MonoBehaviour
{
    public void BackToMenu()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        // โหลดฉาก MainMenu ใหม่ ซึ่งจะทำให้ UI เก่าหายไปทั้งหมดอัตโนมัติ
        SceneManager.LoadScene("MainMenu_New");
    }
}