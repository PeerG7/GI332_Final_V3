using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Player 1 UI")]
    public Slider hpSliderP1;
    public TMP_Text nameP1;

    [Header("Player 2 UI")]
    public Slider hpSliderP2;
    public TMP_Text nameP2;

    // ฟังก์ชันอัปเดตเลือด โดยระบุว่าเป็น Player ไหน (1 หรือ 2)
    public void UpdateHealth(int playerNumber, float currentHealth, float maxHealth)
    {
        if (playerNumber == 1 && hpSliderP1 != null)
        {
            hpSliderP1.maxValue = maxHealth;
            hpSliderP1.value = currentHealth;
        }
        else if (playerNumber == 2 && hpSliderP2 != null)
        {
            hpSliderP2.maxValue = maxHealth;
            hpSliderP2.value = currentHealth;
        }
    }
}