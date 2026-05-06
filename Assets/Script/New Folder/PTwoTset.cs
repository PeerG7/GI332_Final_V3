using UnityEngine;

public class PTwoTset : MonoBehaviour
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

        if (Input.GetKey(KeyCode.UpArrow)) v = 1;
        if (Input.GetKey(KeyCode.DownArrow)) v = -1;
        if (Input.GetKey(KeyCode.LeftArrow)) h = -1;
        if (Input.GetKey(KeyCode.RightArrow)) h = 1;

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
