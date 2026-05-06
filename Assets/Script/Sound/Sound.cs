using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Sound : MonoBehaviour
{
    public static Sound Instance { get; private set; }

    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton — ถ้ามีอยู่แล้วให้ทำลายตัวซ้ำทิ้ง
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // ← ข้าม Scene ได้
        audioSource = GetComponent<AudioSource>();
    }

    // เปลี่ยนเพลง
    public void PlayClip(AudioClip clip, bool loop = true)
    {
        if (clip == null || audioSource.clip == clip) return;
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.Play();
    }

    // หยุดเพลง
    public void Stop() => audioSource.Stop();

    // เพิ่ม/ลดเสียง (0.0 - 1.0)
    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }

    // หยุดชั่วคราว / เล่นต่อ
    public void Pause() => audioSource.Pause();
    public void Resume() => audioSource.UnPause();
}