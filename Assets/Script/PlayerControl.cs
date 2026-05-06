using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerControl : MonoBehaviour
{
    public float speed = 10f;
    private Rigidbody rb;

    // สร้างตัวแปรอ้างอิงคลาสที่ Unity สร้างให้จากรูป
    private InputSystem_Actions controls;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controls = new InputSystem_Actions(); // ชื่อต้องตรงกับไฟล์ในรูปของคุณ
        controls.Player.Enable();
    }
    void Update()
    {
        // อ่านค่า Vector 2 จาก Action "Move" ในรูปที่คุณส่งมา
        moveInput = controls.Player.Move.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // นำค่ามาสร้างทิศทาง x และ z เพื่อกลิ้งลูกบอล
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        rb.AddForce(movement * speed);
    }

}
