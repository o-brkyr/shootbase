using Sandbox;

using static System.Math;

namespace Shootbase.Behaviours.Weapons.Magazine;

[Prefab]
public partial class Clip : BaseMagazineBehaviour
{
    [Prefab, Net, Local, MinMax(1, int.MaxValue), Category("Ammo")]
    public int Capacity { get; protected set; }

    protected int Current { get; set; } = 0;

    public override void Take(int amount)
    {
        Current -= Min(amount, Current);
    }

    public override void Add(int amount)
    {
        Current = Min(amount + Current, Capacity);
    }

    public override int GetCurrent()
    {
        return Current;
    }

    public override bool IsFull()
    {
        return Current == Capacity;
    }

    public override int GetAmmoRemainingToFill()
    {
        return Capacity - Current;
    }

    public override bool IsEmpty()
    {
        return Current == 0;
    }
}
