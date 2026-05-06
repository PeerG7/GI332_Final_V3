using UnityEngine;

public class PTwoTestTwo : MonoBehaviour
{
    public float Speed = 5f;

    void Update()
    {
        float moveX = 0;
        float moveZ = 0;

        // เช็กปุ่มลูกศร
        if (Input.GetKey(KeyCode.UpArrow)) moveZ = 1;
        if (Input.GetKey(KeyCode.DownArrow)) moveZ = -1;
        if (Input.GetKey(KeyCode.LeftArrow)) moveX = -1;
        if (Input.GetKey(KeyCode.RightArrow)) moveX = 1;

        Vector3 direction = new Vector3(moveX, 0, moveZ).normalized;

        // เปลี่ยนตำแหน่งโดยการบวกค่า
        transform.position += direction * Speed * Time.deltaTime;
    }
}
