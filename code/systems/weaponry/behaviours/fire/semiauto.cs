using Shootbase.Behaviours.Weapons.Fire;

namespace Sandbox.systems.weaponry.behaviours.firebehaviour;

[Prefab]
public partial class SemiAuto : BaseFireBehaviour
{
    public override bool ShouldFire()
    {
        return Input.Pressed(InputFire);
    }
}
