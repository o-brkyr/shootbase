using Sandbox;
using System;

namespace Shootbase.Behaviours.Weapons.Fire;

[Prefab]
public partial class Burst : BaseFireBehaviour
{
    [Prefab, Net, Local, MinMax(1, 10), Category("Burst")]
    public int BurstCount { get; set; } = 1;

    [Prefab, Net, Local, Range(0, 4, 0.05f), Category("Burst")]
    protected float BurstDelay { get; set; } = 0;

    [Prefab, Net, Local, Category("Burst")]
    public bool IsAutomatic { get; set; } = false;

    [Net, Predicted]
    protected bool IsBurstActive { get; set; } = false;

    [Net, Predicted]
    protected int CurrentBurstCount { get; set; } = 0;

    [Net, Predicted]
    protected TimeSince TimeSinceLastBurst { get; set; } = 0;

    public override bool ShouldFire()
    {
        if (IsBurstActive)
        {
            return true;
        }

        return IsAutomatic ? Input.Down(InputFire) : Input.Pressed(InputFire);
    }

    protected void FinishBurst()
    {
        IsBurstActive = false;
        CurrentBurstCount = 0;
        TimeSinceLastBurst = 0;
        Log.Info($"Setting next until block time to {BurstDelay} ");
        Weapon.SetNextUnblockTime(BurstDelay);
    }

    public override bool CanFire()
    {
        if (
            HasRunOutOfAmmo()
            || IsBlockedByReload()
            || IsBurstActive && CurrentBurstCount > BurstCount - 1
        )
        {
            FinishBurst();
        }

        if (IsBlockedByReload() || Weapon.IsBlocking())
        {
            return false;
        }

        if (IsBurstActive)
        {
            return base.CanFire();
        }
        else
        {
            return TimeSinceLastBurst >= BurstDelay;
        }
    }

    public override void PreFire()
    {
        if (!IsBurstActive)
        {
            IsBurstActive = true;
        }
        CurrentBurstCount += 1;
        base.PreFire();
    }
}
