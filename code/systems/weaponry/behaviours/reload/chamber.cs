using Sandbox;

namespace Shootbase.Behaviours.Weapons.Reload;

[Prefab]
public partial class Chamber : BaseReloadBehaviour
{
    [Prefab, Net, Local, MinMax(1, 10), Category("Ammo")]
    public int RoundsPerChamber { get; protected set; }

    [Prefab, Net, Local, Category("Animation")]
    protected string ViewReloadFinishParameter { get; set; }

    [Prefab, Net, Local, Category("Animation")]
    protected string WorldReloadFinishParameter { get; set; }

    [Prefab, Net, Local, MinMax(0, 10), Category("Behaviour")]
    public float StartDuration { get; protected set; }

    [Prefab, Net, Local, MinMax(0, 10), Category("Behaviour")]
    public float EndDuration { get; protected set; }

    protected override void OnIdleState()
    {
        if (CanReload())
        {
            IsBlockingOtherActions = true;
            State = ReloadState.Start;
            TimeUntilNextReload = StartDuration;
            Weapon?.playAnimation(ViewReloadParameter, WorldReloadParameter);
        }
        else
        {
            Log.Warning("Can't reload for some odd reason?");
        }
    }

    protected override void OnStartState()
    {
        if (TimeUntilNextReload)
        {
            State = ReloadState.Reload;
            TimeUntilNextReload = ReloadPeriod;
            Weapon?.playAnimation(ViewReloadParameter, WorldReloadParameter);
        }
    }

    public override void MarkCancel()
    {
        if (State == ReloadState.Reload && !IsAwaitingCancel)
        {
            Log.Info("Marking as cancel");
            IsAwaitingCancel = true;
        }
    }

    protected override void OnReloadState()
    {
        if (TimeUntilNextReload)
        {
            // Take ammo from reserve and put into magazine
            int delta = Weapon.Pawn.AmmoStorage.TakeAmmoForType(AmmoType, RoundsPerChamber);
            Log.Info(
                $"Mag: {MagazineRef.GetCurrent()} Reserve: {Weapon.Pawn.AmmoStorage.GetHeldAmmoForType(AmmoType)} [{delta}]"
            );
            MagazineRef.Add(1);

            bool shouldContinueReloading =
                !MagazineRef.IsFull() && Weapon.Pawn.AmmoStorage.GetHeldAmmoForType(AmmoType) > 0;

            if (shouldContinueReloading && !IsAwaitingCancel)
            {
                // Reset timing
                TimeUntilNextReload = ReloadPeriod;
                TimeUntilUnblock = ReloadPeriod;

                // Play animation
                Weapon?.playAnimation(ViewReloadParameter, WorldReloadParameter);
            }
            else
            {
                // Reset timing
                TimeUntilNextReload = 0;
                TimeUntilUnblock = 0;
                IsAwaitingCancel = false;
                State = ReloadState.Finish;
            }
        }
    }

    protected override void OnFinishState()
    {
        Weapon?.playAnimation(ViewReloadFinishParameter, WorldReloadFinishParameter);
        State = ReloadState.Idle;
    }
}
