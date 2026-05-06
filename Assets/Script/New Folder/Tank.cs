using UnityEngine;

public class Tank : Player
{
    private float buffDuration = 5f;
    private int DefBoost = 5;
    private int SpeedBoost = 5;
    private bool isBuffActive = false;
    private float buffEndTime = 0f;
    private int originalDef;
    private float originalSpeed;
    protected override void Start()
    {
        if (IsOwner)
        {
            Hp.Value = 100;
        }
        Speed = 10f;
        Def = 1;
        AtkPower = 20;
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
