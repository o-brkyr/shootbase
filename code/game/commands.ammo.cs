using Sandbox;
using Shootbase.Ammotypes;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace MyGame;

/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client.
///
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
public partial class MyGame
{
    [ConCmd.Admin("give_ammo")]
    public static void GiveAmmo(string ammoName, int quantity)
    {
        var callingClient = ConsoleSystem.Caller;

        var owner = ConsoleSystem.Caller.Pawn as Shootbase.Player;

        var ammoResourceName = $"data/ammo/{ammoName.ToLower().Trim()}.ammo";

        if (owner == null)
        {
            Log.Info("Owner is null");
            return;
        }

        AmmoType outAmmoType;
        if (!ResourceLibrary.TryGet<AmmoType>(ammoResourceName, out outAmmoType))
        {
            Log.Warning($"Couldn't load ammo type {ammoResourceName}");
        }
        var delta = owner.AmmoStorage?.AddAmmoForType(outAmmoType, quantity);
        Log.Info($"Gave player {quantity} rounds of {outAmmoType.DisplayName} [{delta}]");
    }

    //[ConCmd.Server("take_ammo")]
    //public static void TakeAmmo( string ammoName, int quantity )
    //{

    //	var callingClient = ConsoleSystem.Caller;

    //	ammoName = ammoName.ToLower();

    //	var owner = ConsoleSystem.Caller.Pawn as Shootbase.Player;

    //	if ( owner == null )
    //	{
    //		Log.Info( "Owner is null" );
    //		return;
    //	}

    //	var aTypeOfAmmo = TypeLibrary.GetType<IAmmoType>( ammoName)?.TargetType;

    //	if ( aTypeOfAmmo == null )
    //	{
    //		Log.Warning( $"Ammotype '{ammoName}' doesn't exist." );
    //	}

    //	var ammoType = TypeLibrary.Create<IAmmoType>( aTypeOfAmmo );

    //	var delta = owner.AmmoStorage?.TakeAmmoForType( ammoType, quantity );
    //	Log.Info( $"Taking player {owner} {quantity} rounds of {ammoType.Name} [{delta}]" );
    //}

    [ConCmd.Server("list_ammo")]
    public static void PrintAmmo()
    {
        var owner = ConsoleSystem.Caller.Pawn as Shootbase.Player;

        if (owner == null)
        {
            Log.Info("Shocker! owner is null!");
            return;
        }

        if (owner.AmmoStorage == null)
        {
            Log.Warning("Ammo storage is null");
            return;
        }

        if (owner.AmmoStorage.AmmoTypeToQuantity == null)
        {
            Log.Warning("Ammo storage dict hasn't been initialised yhet");
            return;
        }

        foreach (var kp in owner?.AmmoStorage.AmmoTypeToQuantity)
        {
            var key = kp.Key;
            Shootbase.PlayerComponents.AmmoStorageData item = kp.Value;

            Log.Info($"{key} -> {item.Quantity} [max {item.Max}]");
        }
    }

    //[ConCmd.Admin( "give_weapon" )]
    //public static void GiveWeapon( string weaponName )
    //{

    //	var callingClient = ConsoleSystem.Caller;

    //	weaponName = weaponName.ToLower();


    //	var owner = ConsoleSystem.Caller.Pawn as Shootbase.Player;

    //	if ( owner == null )
    //	{
    //		Log.Info( "Owner is null" );
    //		return;
    //	}

    //IAmmoType ammoType;

    //var thing = TypeLibrary.GetType<IAmmoType>();

    //switch ( ammoName )
    //{
    //	case "pistol":
    //		ammoType = new Shootbase.Ammotypes.Pistol();
    //		break;
    //	case "buckshot":
    //		ammoType = new Shootbase.Ammotypes.Buckshot();
    //		break;
    //	case "rpg":
    //		ammoType = new Shootbase.Ammotypes.RPG();
    //		break;
    //	default:
    //		return;

    //}
    //var delta = owner.AmmoStorage?.AddAmmoForType( ammoType, quantity );
    //Log.Info( $"Gave player {quantity} rounds of {ammoType.Name} [{delta}]" );
}
