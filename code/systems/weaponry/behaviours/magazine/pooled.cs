using Sandbox;

namespace Shootbase.Behaviours.Weapons.Magazine;

[Prefab]
public partial class Pooled : BaseMagazineBehaviour
{
    public override int GetCurrent()
    {
        return Weapon?.Pawn?.AmmoStorage?.GetHeldAmmoForType(AmmoType) ?? -1;
    }

    public override bool IsFull()
    {
        return true;
    }

    public override bool IsEmpty()
    {
        return false;
    }
}
