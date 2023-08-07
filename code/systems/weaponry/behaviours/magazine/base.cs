using Sandbox;

namespace Shootbase.Behaviours.Weapons.Magazine;

public partial class BaseMagazineBehaviour : EntityComponent<Shootbase.Weapons.WeaponBase>
{
    [Prefab, Net, Local]
    public Shootbase.Ammotypes.AmmoType AmmoType { get; protected set; }

    protected Shootbase.Weapons.WeaponBase Weapon => Entity;

    public virtual void Take(int amount) { }

    public virtual void Add(int amount) { }

    public virtual int GetAmmoRemainingToFill()
    {
        return 0;
    }

    public virtual bool IsFull()
    {
        return false;
    }

    public virtual bool IsEmpty()
    {
        return false;
    }

    public virtual int GetCurrent()
    {
        return -1;
    }
}
