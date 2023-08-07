using Sandbox;

namespace Shootbase.Behaviours.Weapons.Magazine;

[Prefab, Icon("luggage")]
public partial class Infinite : BaseMagazineBehaviour
{
    public override int GetCurrent()
    {
        return int.MaxValue;
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
