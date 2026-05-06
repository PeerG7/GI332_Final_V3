using UnityEngine;

public class StopHelper : MonoBehaviour
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ResetAfter(float time)
    {
        // ยกเลิกการเรียกเดิมที่อาจจะค้างอยู่ (ถ้ามี)
        CancelInvoke("StopMovement");
        // สั่งให้เรียกฟังก์ชัน StopMovement หลังจากผ่านไป 'time' วินาที
        Invoke("StopMovement", time);
    }

    void StopMovement()
    {
        if (rb != null)
        {
            // หยุดความเร็วของ Rigidbody ให้เป็น 0
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // คืนค่าความหนืด (Damping) ให้เป็นปกติ (เช่น 0)
            rb.linearDamping = 0f;
        }
    }
}
