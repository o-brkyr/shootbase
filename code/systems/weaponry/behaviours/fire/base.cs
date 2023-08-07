using Sandbox;
using Shootbase.Behaviours.Weapons.Magazine;
using Shootbase.Behaviours.Weapons.Reload;

namespace Shootbase.Behaviours.Weapons.Fire;

public enum FireTrigger : ushort
{
    PrimaryFire,
    SecondaryFire,
    TertiaryFire
}

public static class FireTriggerHelper
{
    public static string GetInputFromFireTrigger(FireTrigger fireTrigger)
    {
        switch (fireTrigger)
        {
            case FireTrigger.PrimaryFire:
            {
                return "attack1";
            }
            case FireTrigger.SecondaryFire:
            {
                return "attack2";
            }
            case FireTrigger.TertiaryFire:
            {
                return "attack3";
            }
            default:
                return "attack1";
        }
    }
}

public partial class BaseFireBehaviour : SimulateComponent<Shootbase.Weapons.WeaponBase>
{
    [Prefab, Net, Local, Category("Ammo")]
    public Shootbase.Ammotypes.AmmoType AmmoType { get; protected set; }

    [Prefab, Net, Local, MinMax(0, 10), Category("Ammo")]
    protected int ConsumptionPerShot { get; set; } = 1;

    [Prefab, Net, Local, Category("Animation")]
    protected string ViewFireParameter { get; set; }

    [Prefab, Net, Local, Category("Animation")]
    protected string WorldFireParameter { get; set; }

    [Prefab, Net, Local, Range(0, 5, 0.05f), Category("Behaviour")]
    protected float PreFireDelay { get; set; } = 0;

    [Prefab, Net, Local, Range(0, 5, 0.01f), Category("Behaviour")]
    protected float FirePeriod { get; set; } = 0;

    [Prefab, Net, Local, MinMax(1, 20), Category("Behaviour")]
    protected int BulletsPerShot { get; set; } = 1;

    [Prefab, Net, Local, Range(0, 60), Category("Behaviour")]
    protected float FireSpread { get; set; } = 0;

    [Prefab, Net, Local, Category("Input")]
    public FireTrigger Trigger { get; protected set; }

    [Prefab, Net, Local, Category("Sound")]
    protected SoundEvent FireSound { get; set; }

    [Net, Predicted, Local]
    public bool IsActive { get; private set; } = false;

    [Net, Predicted]
    public TimeSince TimesinceLastFire { get; set; } = 0;

    [Net, Local]
    public BaseMagazineBehaviour MagazineRef { get; set; }

    [Net, Local]
    public BaseReloadBehaviour ReloadRef { get; set; }

    protected string InputFire => FireTriggerHelper.GetInputFromFireTrigger(Trigger);

    protected Shootbase.Weapons.WeaponBase Weapon => Entity;

    public override void Simulate(IClient player)
    {
        CheckDryFire();

        if (!Game.IsServer)
            return;

        if (!ShouldFire())
        {
            return;
        }

        if (IsBlockedByReload())
        {
            ReloadRef.MarkCancel();
        }

        if (CanFire())
        {
            PreFire();

            Entity.PlaySound(FireSound.ResourceName);

            using (Sandbox.Entity.LagCompensation())
            {
                OnFire();
                PostFire();
            }
        }
    }

    protected void CheckDryFire()
    {
        if (Input.Pressed(InputFire) && MagazineRef.IsEmpty())
        {
            if (!Game.IsServer)
                return;
            OnDryFire();
        }
    }

    public virtual bool ShouldFire()
    {
        return (Input.Down(InputFire));
    }

    protected bool HasRunOutOfAmmo()
    {
        return MagazineRef != null ? MagazineRef.IsEmpty() : false;
    }

    protected bool IsBlockedByReload()
    {
        return ReloadRef != null ? ReloadRef.IsBlocking() : false;
    }

    public virtual bool CanFire()
    {
        if (HasRunOutOfAmmo() || IsBlockedByReload() || Weapon.IsBlocking())
        {
            return false;
        }
        return TimesinceLastFire >= FirePeriod;
    }

    public virtual void OnFire()
    {
        Weapon.ShootEffects(ViewFireParameter, WorldFireParameter);
        for (int i = 0; i < BulletsPerShot; i++)
        {
            Weapon.ShootBullet(FireSpread, 100, 20, 1, i);
        }
    }

    public virtual void PreFire()
    {
        Log.Info($"Setting next until block time to {FirePeriod} ");
        Weapon.SetNextUnblockTime(FirePeriod);
        TimesinceLastFire = 0;
    }

    public virtual void PostFire()
    {
        if (MagazineRef == null)
        {
            foreach (
                BaseMagazineBehaviour component in Weapon.Components.GetAll<BaseMagazineBehaviour>()
            )
            {
                if (component.AmmoType == AmmoType)
                {
                    MagazineRef = component;
                }
            }
            if (MagazineRef == null)
            {
                Log.Warning("Can't find appropriate magazine component");
                return;
            }
        }
        MagazineRef.Take(ConsumptionPerShot);
        Log.Info($"{MagazineRef.GetCurrent()}");
    }

    public virtual void OnDryFire()
    {
        Entity.PlaySound("wpn.generic.dryfire");
    }

    public virtual void BlockWeapon()
    {
        Weapon.SetNextUnblockTime(FirePeriod);
    }
}
