using UnityEngine;

public class PoneTestTwo : MonoBehaviour
{
    public float Speed = 5f;
    public float SmoothTime = 5f; // ค่าความลื่น (ยิ่งน้อยยิ่งหยุดช้า/ออกตัวช้า)
    private Vector3 currentVelocity; // ความเร็วปัจจุบันที่กำลังไหลอยู่

    void Update()
    {
        float moveX = 0;
        float moveZ = 0;

        if (Input.GetKey(KeyCode.W)) moveZ = 1;
        if (Input.GetKey(KeyCode.S)) moveZ = -1;
        if (Input.GetKey(KeyCode.A)) moveX = -1;
        if (Input.GetKey(KeyCode.D)) moveX = 1;

        // 1. หา "เป้าหมาย" ของทิศทางที่อยากไป
        Vector3 targetDirection = new Vector3(moveX, 0, moveZ).normalized;

        // 2. ใช้ Lerp ค่อยๆ ปรับความเร็วปัจจุบัน (currentVelocity) ให้ไปหาเป้าหมาย
        // วิธีนี้จะทำให้เวลาปล่อยปุ่ม ค่าจะไม่กลายเป็น 0 ทันที แต่จะค่อยๆ ลดลงจนหยุด
        currentVelocity = Vector3.Lerp(currentVelocity, targetDirection, SmoothTime * Time.deltaTime);

        // 3. บวกตำแหน่งด้วยความเร็วที่กำลังไหลอยู่
        transform.position += currentVelocity * Speed * Time.deltaTime;
    }
}
