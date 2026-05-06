using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public UIManager uiManager;

    public void PlayGame()
    {
        // เมื่อกด Play ให้เปิดหน้าเลือกโหมด (Play_Panel)
        uiManager.OpenPanel(uiManager.playPanel);
    }

    public void QuitGame()
    {
        Debug.Log("Quit!");
        Application.Quit();
    }
}