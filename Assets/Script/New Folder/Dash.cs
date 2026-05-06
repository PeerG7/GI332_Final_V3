using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Dash : Player
{
    private float dashForce = 10f;
    protected override void Start()
    {
        transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        if (IsOwner)
        {
            RequestInitializeStatsServerRpc();
        }
        Speed = 15f;
        Def = 2;
        AtkPower = 25;
        SmoothTime = 3f;
    }

    protected override void Class()
    {
        // เช็กปุ่ม Jump (หรือเปลี่ยนเป็นปุ่มอื่นใน Action Asset)
        if (Controls.Player.Jump.WasPressedThisFrame() && Cooldown.Value >= CanCast)
        {
            DoDash();
            ResetCooldownServerRpc();
        }
    }
    private void DoDash()
    {
        Vector3 dashDir = new Vector3(MoveInput.x, 0f, MoveInput.y).normalized;
        if (dashDir == Vector3.zero)
        {
            dashDir = transform.forward;
        }
        rb.AddForce(dashDir * dashForce, ForceMode.Impulse);
        Speed *= 3f;
        helper = GetComponent<StopHelper>();
        helper.ResetAfter(1f);
        Invoke("ResetSpeed", 3f);
    }

    private float originalSpeed;
    private void ResetSpeed()
    {
        Speed /= 3f;
    }
}
