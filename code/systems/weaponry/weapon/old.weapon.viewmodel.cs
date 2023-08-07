using Sandbox;

namespace Shootbase.Weapons;

public partial class OldWeaponViewModel : BaseViewModel
{
    protected WeaponBase Weapon { get; init; }

    //public WeaponViewModel( WeaponBase weapon )
    //{
    //	Weapon = weapon;
    //	EnableShadowCasting = false;
    //	EnableViewmodelRendering = true;
    //}

    public override void PlaceViewmodel()
    {
        base.PlaceViewmodel();

        Camera.Main.SetViewModelCamera(80f, 1, 500);
    }
}
