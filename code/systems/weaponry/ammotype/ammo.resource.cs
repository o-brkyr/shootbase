using Sandbox;
using System.Collections.Generic;

namespace Shootbase.Ammotypes;

[GameResource(
    "Ammo type definition",
    "ammo",
    "Describes a particular type of ammo",
    Icon = "add_shopping_cart"
)]
public partial class AmmoType : GameResource
{
    [ResourceType("vmdl")]
    public string WorldModel { get; set; }

    public string DisplayName { get; set; }
    public string LongName { get; set; }

    [MinMax(0, int.MaxValue)]
    public int DefaultPickupAmount { get; set; }

    [MinMax(0, int.MaxValue)]
    public int MaxAmmo { get; set; } = int.MaxValue;

    public static IReadOnlyList<AmmoType> All => _all;
    internal static List<AmmoType> _all = new();

    protected override void PostLoad()
    {
        base.PostLoad();

        if (!_all.Contains(this))
        {
            _all.Add(this);
        }
    }
}
