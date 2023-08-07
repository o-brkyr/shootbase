using Sandbox;
using Shootbase.Weapons;
using System.Collections.Generic;
using System.Linq;

namespace Shootbase.PlayerComponents;

public partial class Inventory : SimulateComponent<Player>, ISingletonComponent
{
    [Net, Predicted, Local]
    public IList<WeaponBase> Weapons { get; protected set; }

    [Net, Predicted, Local]
    public WeaponBase ActiveWeapon { get; protected set; }

    [Net, Predicted, Local]
    public int ActiveWeaponIndex { get; protected set; }

    [Net, Predicted, Local]
    protected int NextSlot { get; set; }

    [Net, Predicted, Local]
    protected TimeUntil TimeUntilCanSwitch { get; set; }

    [Net, Predicted, Local]
    protected bool IsSwitching { get; set; }

    protected Player Pawn => Entity;

    public int AddWeapon(WeaponBase weapon)
    {
        Weapons.Add(weapon);
        return Weapons.Count() - 1;
    }

    public void RemoveWeapon(WeaponBase weapon)
    {
        if (!HasWeapon(weapon))
        {
            return;
        }
        while (Weapons.Contains(weapon))
        {
            Weapons.Remove(weapon);
        }
    }

    public bool HasWeapon(WeaponBase weapon)
    {
        return Weapons.Contains(weapon);
    }

    public bool HasWeaponPrefab(string prefabName)
    {
        var prefab = ResourceLibrary
            .GetAll<Prefab>()
            .FirstOrDefault(prefab => prefab.ResourceName.ToLower() == prefabName.ToLower());
        Log.Info($"ResourceName: {prefab.ResourceName}, RootEntry: {prefab.GetHashCode()}");
        return true;
    }

    public WeaponBase? GetWeaponAtSlot(int slot)
    {
        return Weapons.ElementAtOrDefault(slot);
    }

    public void ListWeapons()
    {
        for (var i = 0; i < Weapons.Count; i++)
        {
            Log.Info($"[Slot {i}] -> {Weapons[i].Name}");
        }
    }

    protected void SetupSwitching()
    {
        if (!IsSwitching)
        {
            IsSwitching = true;
            TimeUntilCanSwitch = ActiveWeapon != null ? ActiveWeapon.HolsterDuration : 0;
            ActiveWeapon?.OnHolster();
        }
    }

    public void SwitchToSlot(int slot)
    {
        if (GetWeaponAtSlot(slot) == null)
        {
            Log.Warning("No weapon at that slot.");
            return;
        }

        SetupSwitching();
        NextSlot = slot;
    }

    public void SwitchToNextWeapon()
    {
        if (Weapons.Count <= 1)
        {
            return;
        }
        int nextSlot = (ActiveWeaponIndex + 1) % Weapons.Count;
        SwitchToSlot(nextSlot);
    }

    public void SwitchToPreviousWeapon()
    {
        if (Weapons.Count <= 1)
        {
            return;
        }
        int nextSlot = (ActiveWeaponIndex - 1 + Weapons.Count) % Weapons.Count;
        SwitchToSlot(nextSlot);
    }

    public override void Simulate(IClient player)
    {
        ActiveWeapon?.Simulate(player);

        if (!IsSwitching)
            return;

        if (TimeUntilCanSwitch)
        {
            Log.Info("Switching to");
            ActiveWeapon = GetWeaponAtSlot(NextSlot);
            ActiveWeapon.OnEquip(Pawn);
            ActiveWeaponIndex = NextSlot;
            IsSwitching = false;
        }
    }
}
