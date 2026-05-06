using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelector : MonoBehaviour
{
    [Header("Data")]
    public CharacterData[] characters; // ลากไฟล์ Character Data ที่สร้างไว้มาใส่ในนี้
    private int currentIndex = 0;

    [Header("UI References")]
    public Image displayImage;   // ช่อง Pic ในรูปของคุณ
    public TextMeshProUGUI nameText; // ช่อง name ในรูปของคุณ

    void Start()
    {
        UpdateDisplay();
    }

    public void NextCharacter()
    {
        currentIndex = (currentIndex + 1) % characters.Length;
        UpdateDisplay();
    }

    public void PreviousCharacter()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = characters.Length - 1;
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        displayImage.sprite = characters[currentIndex].characterIcon;
        nameText.text = characters[currentIndex].characterName;

        // เก็บค่า Index ไว้ใช้ตอนเกิดในเกม
        PlayerPrefs.SetInt("SelectedCharacter", currentIndex);
    }
}