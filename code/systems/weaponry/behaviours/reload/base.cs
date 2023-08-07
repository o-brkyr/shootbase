using Sandbox;
using Shootbase.Behaviours.Weapons.Magazine;

namespace Shootbase.Behaviours.Weapons.Reload;

public enum ReloadState
{
    Idle,
    Start,
    Reload,
    Finish
}

public partial class BaseReloadBehaviour : SimulateComponent<Shootbase.Weapons.WeaponBase>
{
    [Prefab, Net, Local, Category("Animation")]
    protected string ViewReloadParameter { get; set; }

    [Prefab, Net, Local, Category("Animation")]
    protected string WorldReloadParameter { get; set; }

    [Prefab, Net, Local, MinMax(0, 10), Category("Behaviour")]
    public float ReloadPeriod { get; protected set; }

    [Prefab, Net, Local, Category("Behaviour")]
    public Shootbase.Ammotypes.AmmoType AmmoType { get; protected set; }

    [Prefab, Net, Local]
    public bool Cancellable { get; protected set; }

    [Net, Local]
    protected bool IsAwaitingCancel { get; set; } = false;

    [Net, Local]
    protected bool IsBlockingOtherActions { get; set; } = false;

    [Net, Local]
    public BaseMagazineBehaviour MagazineRef { get; set; }

    [Net, Local]
    protected ReloadState State { get; set; } = ReloadState.Idle;

    [Net, Local, Predicted]
    protected TimeUntil TimeUntilNextReload { get; set; } = 0;

    [Net, Local, Predicted]
    protected TimeUntil TimeUntilUnblock { get; set; } = 0;

    protected Shootbase.Weapons.WeaponBase Weapon => Entity;

    public virtual bool CanReload()
    {
        if (MagazineRef == null)
            return false;
        if (Weapon?.Pawn?.AmmoStorage == null)
            return false;
        if (Weapon.IsBlocking())
            return false;
        return TimeUntilNextReload <= 0
            && !MagazineRef.IsFull()
            && Weapon.Pawn.AmmoStorage.GetHeldAmmoForType(AmmoType) > 0;
    }

    public virtual bool ShouldReload()
    {
        return Input.Pressed("reload") || State != ReloadState.Idle;
    }

    public virtual bool IsReloading()
    {
        return State != ReloadState.Idle;
    }

    public virtual bool IsBlocking()
    {
        return !(State == ReloadState.Finish || State == ReloadState.Idle);
    }

    public virtual void MarkCancel() { }

    protected virtual void OnUnblock()
    {
        Log.Info("Unblocking");
        Log.Info($"TimeUntilUnblock = {TimeUntilUnblock}");
    }

    protected virtual void OnIdleState()
    {
        if (CanReload())
        {
            State = ReloadState.Start;
            IsBlockingOtherActions = true;
        }
        else
        {
            Log.Warning("Can't reload for some reason");
        }
    }

    protected virtual void OnStartState()
    {
        State = ReloadState.Reload;
        TimeUntilNextReload = ReloadPeriod;
        Weapon?.playAnimation(ViewReloadParameter, WorldReloadParameter);
    }

    protected virtual void OnReloadState()
    {
        // Play a reload animation, at the end add ammunition
        if (TimeUntilNextReload)
        {
            int ammoToTake = MagazineRef.GetAmmoRemainingToFill();

            if (ammoToTake > 0)
            {
                // Take ammo from reserve and put into magazine
                int delta = Weapon.Pawn.AmmoStorage.TakeAmmoForType(AmmoType, ammoToTake);
                Log.Info($"Took {delta} rounds");
                MagazineRef.Add(ammoToTake);
            }

            // Reset timing
            TimeUntilNextReload = 0;
            TimeUntilUnblock = 0;
            State = ReloadState.Idle;
            IsBlockingOtherActions = false;
        }
    }

    protected virtual void OnFinishState() { }

    protected virtual void CheckState()
    {
        if (State == ReloadState.Idle)
        {
            // Transition to reload state
            OnIdleState();
        }
        if (State == ReloadState.Start)
        {
            // Play deploy animations, if any
            OnStartState();
        }
        if (State == ReloadState.Reload)
        {
            // Transition to Finish
            OnReloadState();
        }
        if (State == ReloadState.Finish)
        {
            OnFinishState();
        }
    }

    public override void Simulate(IClient player)
    {
        if (TimeUntilUnblock && IsBlockingOtherActions)
        {
            IsBlockingOtherActions = false;
        }

        if (!ShouldReload())
        {
            return;
        }

        if (!Game.IsServer)
            return;

        CheckState();
    }
}
