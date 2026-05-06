using UnityEngine;

public class Tank : Player
{
    private float buffDuration = 5f;
    private int DefBoost = 7;
    private int SpeedBoost = 3;
    private bool isBuffActive = false;
    private float buffEndTime = 0f;
    private int originalDef;
    private float originalSpeed;
    protected override void Start()
    {
        transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        if (IsOwner)
        {
            RequestInitializeStatsServerRpc();
        }
        Speed = 7f;
        Def = 3;
        AtkPower = 15;
        SmoothTime = 3f;
        originalDef = Def;
        originalSpeed = Speed;
    }
    protected override void Class()
    {
        if (Controls.Player.Jump.WasPressedThisFrame() && Cooldown.Value >= CanCast)
        {
            ActivateDefBuff();
            ResetCooldownServerRpc();
        }
        if (isBuffActive && Time.time >= buffEndTime)
        {
            DeactivateDefBuff();
        }
    }
    private void ActivateDefBuff()
    {
        isBuffActive = true;
        buffEndTime = Time.time + buffDuration;
        Def += DefBoost;
        Speed += SpeedBoost;
    }

    private void DeactivateDefBuff()
    {
        isBuffActive = false;
        Def = originalDef;
        Speed = originalSpeed;
    }
}
