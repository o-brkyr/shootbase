using Sandbox;
using System;
using System.Collections.Generic;

namespace Shootbase.PlayerComponents;

public readonly struct AmmoStorageData
{
    public AmmoStorageData(int quantity, int max)
    {
        Quantity = quantity;
        Max = max;
    }

    public int Quantity { get; init; }
    public int Max { get; init; }
}

public partial class AmmoStorage : EntityComponent<Player>, ISingletonComponent
{
    [Net, Predicted]
    public IDictionary<string, AmmoStorageData> AmmoTypeToQuantity { get; set; } =
        new Dictionary<string, AmmoStorageData>();

    public void ClearStorage()
    {
        AmmoTypeToQuantity.Clear();
    }

    public int GetHeldAmmoForType(Ammotypes.AmmoType ammoType)
    {
        string ammoTypeName = ammoType.ResourceName;

        // If AmmoToCapacity hasn't been initialised, return 0
        if (AmmoTypeToQuantity == null)
        {
            Log.Warning("Ammo type to capacity dict isn't initilaise");
            return 0;
        }

        AmmoStorageData ammoData;
        if (!AmmoTypeToQuantity.TryGetValue(ammoTypeName, out ammoData))
            return 0;
        return ammoData.Quantity;
    }

    public int SetHeldAmmoForType(Ammotypes.AmmoType ammoType, int quantity)
    {
        // Don't do shit if we're on the client
        //if ( !Game.IsServer ) return 0;

        // Force a positive value
        quantity = Math.Max(quantity, 0);

        int delta;
        string ammoTypeName = ammoType.ResourceName;

        AmmoStorageData ammoData;
        if (!AmmoTypeToQuantity.TryGetValue(ammoTypeName, out ammoData))
        {
            var newValue = Math.Min(quantity, ammoType.MaxAmmo);
            delta = newValue;
            ammoData = new AmmoStorageData(newValue, ammoType.MaxAmmo);
        }
        else
        {
            var newValue = Math.Min(quantity, ammoData.Max);
            delta = newValue - ammoData.Quantity;
            ammoData = new AmmoStorageData(newValue, ammoData.Max);
        }
        AmmoTypeToQuantity[ammoTypeName] = ammoData;
        return delta;
    }

    public int AddAmmoForType(Ammotypes.AmmoType ammoType, int quantity)
    {
        int currentQuantity = GetHeldAmmoForType(ammoType);

        // Check if overflows
        if (int.MaxValue - currentQuantity < quantity)
        {
            return SetHeldAmmoForType(ammoType, int.MaxValue);
        }

        return SetHeldAmmoForType(ammoType, currentQuantity + quantity);
    }

    public int TakeAmmoForType(Ammotypes.AmmoType ammoType, int quantity)
    {
        var currentQuantity = GetHeldAmmoForType(ammoType);

        if (currentQuantity == 0)
            return 0;

        if (quantity >= currentQuantity)
        {
            return SetHeldAmmoForType(ammoType, 0);
        }

        return SetHeldAmmoForType(ammoType, currentQuantity - quantity);
    }

    public void SetMaxForAmmoType(Ammotypes.AmmoType ammoType, int max)
    {
        //// Don't do shit if we're on the client
        //if ( !Game.IsServer ) return;

        max = Math.Max(max, 0);

        string ammoTypeName = ammoType.ResourceName;

        AmmoStorageData ammoData;
        if (!AmmoTypeToQuantity.TryGetValue(ammoTypeName, out ammoData))
        {
            ammoData = new AmmoStorageData(0, max);
        }
        else
        {
            ammoData = new AmmoStorageData(ammoData.Quantity, max);
        }

        AmmoTypeToQuantity[ammoTypeName] = ammoData;
    }

    public int GetMaxForAmmoType(Ammotypes.AmmoType ammoType)
    {
        string ammoTypeName = ammoType.ResourceName;
        AmmoStorageData ammoData;
        if (!AmmoTypeToQuantity.TryGetValue(ammoTypeName, out ammoData))
        {
            // Return ammo type's default if it hasn't been instantiated yet
            return ammoType.MaxAmmo;
        }
        return ammoData.Max;
    }
}
