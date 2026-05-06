using Unity.Netcode;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Jumper : Player
{
    private float jumpForce = 10f;
    private bool isGrounded;
    protected override void Start()
    {
        if (IsOwner)
        {
            RequestInitializeStatsServerRpc();
        }
        Speed = 10f;
        Def = 1;
        AtkPower = 20;
        SmoothTime = 3f;
    }
    protected override void Class()
    {
        if (Controls.Player.Jump.WasPressedThisFrame() && isGrounded && Cooldown.Value >= CanCast)
        {
            Jump();
            ResetCooldownServerRpc();
        }
    }
    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
        
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (IsOwner && Hp.Value <= 0)
        {
            Die();
        }
    }
    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    
}
