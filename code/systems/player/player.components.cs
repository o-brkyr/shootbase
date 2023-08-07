using Sandbox;
using Shootbase.PlayerComponents;
using System.ComponentModel;

namespace Shootbase;

public partial class Player : AnimatedEntity
{
    [BindComponent]
    public Inventory Inventory { get; }

    [BindComponent]
    public AmmoStorage AmmoStorage { get; }

    [BindComponent]
    public PlayerController Controller { get; }

    [BindComponent]
    public PlayerAnimator Animator { get; }

    protected void CreateComponents()
    {
        Components.Create<PlayerController>();
        Components.Create<PlayerAnimator>();
        Components.Create<AmmoStorage>();
        Components.Create<Inventory>();
    }

    protected void SimulateComponents(IClient cl)
    {
        // SimulateRotation();

        Controller?.Simulate(cl);
        Animator?.Simulate();

        // EyeLocalPosition;

        if (Input.Pressed("Slot1"))
        {
            Inventory?.SwitchToPreviousWeapon();
        }
        else if (Input.Pressed("Slot2"))
        {
            Inventory?.SwitchToNextWeapon();
        }

        Inventory?.Simulate(cl);
    }
}
