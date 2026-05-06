using UnityEngine;
using UnityEngine.UI;

public class AudioToggleButton : MonoBehaviour
{
    public static AudioToggleButton Instance { get; private set; }

    [Header("References")]
    public Button toggleButton;   // ลาก Button มาใส่
    public Image iconOn;          // ไอคอนรูปลำโพงเปิด
    public Image iconOff;         // ไอคอนรูปลำโพงปิด (มีขีด)

    private bool isMuted = false;
    private const string MUTE_KEY = "AudioMuted"; // เก็บสถานะไว้ใน PlayerPrefs

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // ← ข้าม Scene ได้
    }

    private void Start()
    {
        // โหลดสถานะล่าสุดที่บันทึกไว้
        isMuted = PlayerPrefs.GetInt(MUTE_KEY, 0) == 1;
        ApplyMute();

        // ผูก event กับปุ่ม
        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleAudio);
    }

    public void ToggleAudio()
    {
        isMuted = !isMuted;
        PlayerPrefs.SetInt(MUTE_KEY, isMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMute();
    }

    private void ApplyMute()
    {
        // ปรับเสียงทั้งเกม
        AudioListener.volume = isMuted ? 0f : 1f;

        // สลับ Icon
        if (iconOn != null) iconOn.gameObject.SetActive(!isMuted);
        if (iconOff != null) iconOff.gameObject.SetActive(isMuted);
    }

    private void OnDestroy()
    {
        if (toggleButton != null)
            toggleButton.onClick.RemoveListener(ToggleAudio);
    }
}