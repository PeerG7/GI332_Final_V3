using UnityEngine;

public class POneTest : MonoBehaviour
{
    public float Speed = 5f;
    public int Def = 1; // ใช้กำหนดน้ำหนัก (Mass)
    private Rigidbody rb;
    private Vector3 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.mass = Def; // ตั้งน้ำหนักตามค่า Def
    }

    void Update()
    {
        float h = 0;
        float v = 0;

        if (Input.GetKey(KeyCode.W)) v = 1;
        if (Input.GetKey(KeyCode.S)) v = -1;
        if (Input.GetKey(KeyCode.A)) h = -1;
        if (Input.GetKey(KeyCode.D)) h = 1;

        movement = new Vector3(h, 0, v).normalized;
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.AddForce(movement * Speed); // ใส่แรงเคลื่อนที่
        }
    }
}
