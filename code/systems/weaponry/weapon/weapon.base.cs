using Sandbox;
using Shootbase.Ammotypes;
using Shootbase.Behaviours.Weapons.Fire;
using Shootbase.Behaviours.Weapons.Magazine;
using Shootbase.Behaviours.Weapons.Reload;
using System.Collections.Generic;
using System.Linq;

namespace Shootbase.Weapons;

[Prefab, Title("BaseWeapon"), Icon("precision_manufacturing")]
public partial class WeaponBase : AnimatedEntity
{
    public WeaponViewModel ViewModelEntity { get; protected set; }

    public Player Pawn => Owner as Player;

    public AnimatedEntity EffectEntity =>
        Camera.FirstPersonViewer == Owner ? ViewModelEntity : this;

    [Prefab, Net, Local, Category("Model")]
    public string ViewModelPath { get; set; }

    [Prefab, Net, Local, Category("Model")]
    public bool IsViewmodelCloud { get; set; } = false;

    [Prefab, Net, Local, Category("Model")]
    public string ModelPath { get; set; }

    [Prefab, Net, Local, Category("Model")]
    public bool IsWorldmodelCloud { get; set; } = false;

    [Net, Local]
    public Shootbase.Ammotypes.AmmoType PrimaryAmmoType { get; protected set; }

    [Net, Local]
    public Shootbase.Ammotypes.AmmoType SecondaryAmmoType { get; protected set; }

    [Net, Local]
    public Shootbase.Ammotypes.AmmoType TertiaryAmmoType { get; protected set; }

    [Net, Local]
    public BaseMagazineBehaviour PrimaryMagRef { get; protected set; }

	[Prefab, Net, Category( "Animation" )]
	public float HolsterDuration { get; set; } = 0;

	[Prefab, Net, Category( "Animation" )]
	public float DeployDuration { get; set; } = 0;

    [Net]
    protected TimeUntil TimeUntilUnblockActions { get; set; }

    public bool IsBlocking()
    {
        return (!TimeUntilUnblockActions);
    }

    public void SetNextUnblockTime(float unblockTime)
    {
        TimeUntilUnblockActions = unblockTime;
    }

    public virtual float PrimaryRate => 5.0f;

    public override void Spawn()
    {
        Log.Info("Spawning");

        SetupComponents();
        SetAmmoTypes();

        EnableHideInFirstPerson = true;
        EnableShadowInFirstPerson = true;
        EnableDrawing = false;

        if (ModelPath != null)
        {
            SetModel(ModelPath);
        }

        Sound.FromEntity("ui.favourite", this);
    }

    protected void setAllComponentsState(bool active)
    {
        var allComponents = Components.GetAll<EntityComponent>(true);

        foreach (var component in allComponents)
        {
            component.Enabled = active;
        }
    }

    void SetupComponents()
    {
        var allMagazineComponents = Components.GetAll<BaseMagazineBehaviour>();
        var allFiretraitComponents = Components.GetAll<BaseFireBehaviour>();
        var allReloadComponents = Components.GetAll<BaseReloadBehaviour>();

        IDictionary<AmmoType, BaseMagazineBehaviour> AmmoTypesToComponents =
            new Dictionary<AmmoType, BaseMagazineBehaviour>();
        IDictionary<AmmoType, BaseReloadBehaviour> AmmoTypesToReloads =
            new Dictionary<AmmoType, BaseReloadBehaviour>();

        foreach (BaseMagazineBehaviour magComponent in allMagazineComponents)
        {
            AmmoTypesToComponents[magComponent.AmmoType] = magComponent;
        }

        foreach (BaseReloadBehaviour reloadComponent in allReloadComponents)
        {
            BaseMagazineBehaviour magComponent;
            if (!AmmoTypesToComponents.TryGetValue(reloadComponent.AmmoType, out magComponent))
            {
                Log.Error(
                    $"A reloadtrait uses an ammo type not defined by a MagazineComponent ({reloadComponent.AmmoType.ResourceName})"
                );
            }
            else
            {
                reloadComponent.MagazineRef = AmmoTypesToComponents[magComponent.AmmoType];
            }
            AmmoTypesToReloads[reloadComponent.AmmoType] = reloadComponent;
        }

        foreach (BaseFireBehaviour firetraitComponent in allFiretraitComponents)
        {
            BaseMagazineBehaviour magComponent;
            if (!AmmoTypesToComponents.TryGetValue(firetraitComponent.AmmoType, out magComponent))
            {
                Log.Error(
                    $"A firetrait uses an ammo type not defined by a MagazineComponent ({firetraitComponent.AmmoType.ResourceName})"
                );
            }
            else
            {
                firetraitComponent.MagazineRef = AmmoTypesToComponents[magComponent.AmmoType];
                if (firetraitComponent.Trigger == FireTrigger.PrimaryFire)
                {
                    PrimaryMagRef = AmmoTypesToComponents[magComponent.AmmoType];
                }
            }
            BaseReloadBehaviour reloadComponent;
            if (!AmmoTypesToReloads.TryGetValue(firetraitComponent.AmmoType, out reloadComponent))
            {
                Log.Error(
                    $"A firetrait uses an ammo type not defined by a ReloadComponent ({firetraitComponent.AmmoType.ResourceName})"
                );
            }
            else
            {
                firetraitComponent.ReloadRef = reloadComponent;
            }
        }
    }

    void SetAmmoTypes()
    {
        var allComponents = Components.GetAll<BaseFireBehaviour>();

        Log.Info($"All components: {allComponents}");

        foreach (BaseFireBehaviour component in allComponents)
        {
            Log.Info($"For component {component}");
            Log.Info(component.Trigger);
            switch (component.Trigger)
            {
                case (FireTrigger.PrimaryFire):
                {
                    Log.Info($"Setting primary ammo to {component.AmmoType.LongName}");
                    PrimaryAmmoType = component.AmmoType;
                    break;
                }
                case (FireTrigger.SecondaryFire):
                {
                    Log.Info($"Setting secondary ammo to {component.AmmoType.LongName}");
                    SecondaryAmmoType = component.AmmoType;
                    break;
                }
                case (FireTrigger.TertiaryFire):
                {
                    Log.Info($"Setting tertiary ammo to {component.AmmoType.LongName}");
                    TertiaryAmmoType = component.AmmoType;
                    break;
                }
            }
        }

        Log.Info($"Primary ammo type is now {PrimaryAmmoType}");
        Log.Info($"Secondary ammo type is now {SecondaryAmmoType}");
        Log.Info($"Tertiary ammo type is now {TertiaryAmmoType}");
    }

    /// <summary>
    /// Called when <see cref="Pawn.SetActiveWeapon(Weapon)"/> is called for this weapon.
    /// </summary>
    /// <param name="pawn"></param>
    public float OnEquip(Player pawn)
    {
        Log.Info($"Calling OnEquip for {Name}");
        Owner = pawn;
        SetParent(pawn, true);
        EnableDrawing = true;
        Log.Info("EQupping");
        setAllComponentsState(true);

        CreateViewModel(To.Single(pawn));
        playAnimation("deploy", "deploy");

        return DeployDuration;
    }

    /// <summary>
    /// Called when <see cref="Pawn.SetActiveWeapon(Weapon)"/> is called for this weapon.
    /// </summary>
    /// <param name="pawn"></param>
    public float OnHolster()
    {
        Log.Info($"Calling OnHOlster for {Name}");
        EnableDrawing = false;
        DestroyViewModel(To.Single(Owner));
        setAllComponentsState(false);
        return HolsterDuration;
    }

    /// <summary>
    /// Called from <see cref="Pawn.Simulate(IClient)"/>.
    /// </summary>
    /// <param name="player"></param>
    public override void Simulate(IClient player)
    {
        SimulateComponents(player);
    }

    void SimulateComponents(IClient player)
    {
        var allComponents = Components.GetAll<SimulateComponent>();

        foreach (var component in allComponents)
        {
            component.Simulate(player);
        }
    }

    /// <summary>
    /// Does a trace from start to end, does bullet impact effects. Coded as an IEnumerable so you can return multiple
    /// hits, like if you're going through layers or ricocheting or something.
    /// </summary>
    public virtual IEnumerable<TraceResult> TraceBullet(
        Vector3 start,
        Vector3 end,
        float radius = 2.0f
    )
    {
        bool underWater = Trace.TestPoint(start, "water");

        var trace = Trace
            .Ray(start, end)
            .UseHitboxes()
            .WithAnyTags("solid", "player", "npc")
            .Ignore(this)
            .Size(radius);

        //
        // If we're not underwater then we can hit water
        //
        if (!underWater)
            trace = trace.WithAnyTags("water");

        var tr = trace.Run();

        if (tr.Hit)
            yield return tr;
    }

    [ClientRpc]
    public virtual void ShootEffects(string viewmodelAnimation, string worldmodelAnimation)
    {
        Game.AssertClient();

        Particles.Create("particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle");
        Pawn.SetAnimParameter(worldmodelAnimation, true);
        ViewModelEntity?.SetAnimParameter(viewmodelAnimation, true);
    }

    /// <summary>
    /// Shoot a single bullet
    /// </summary>
    public virtual void ShootBullet(
        Vector3 pos,
        Vector3 dir,
        float spread,
        float force,
        float damage,
        float bulletSize
    )
    {
        var forward = dir;
        forward +=
            (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
        forward = forward.Normal;

        //
        // ShootBullet is coded in a way where we can have bullets pass through shit
        // or bounce off shit, in which case it'll return multiple results
        //
        foreach (var tr in TraceBullet(pos, pos + forward * 5000, bulletSize))
        {
            tr.Surface.DoBulletImpact(tr);

            if (!Game.IsServer)
                continue;
            if (!tr.Entity.IsValid())
                continue;

            //
            // We turn predictiuon off for this, so any exploding effects don't get culled etc
            //
            using (Prediction.Off())
            {
                var damageInfo = DamageInfo
                    .FromBullet(tr.EndPosition, forward * 100 * force, damage)
                    .UsingTraceResult(tr)
                    .WithAttacker(Owner)
                    .WithWeapon(this);

                tr.Entity.TakeDamage(damageInfo);
            }
        }
    }

    /// <summary>
    /// Shoot a single bullet from owners view point
    /// </summary>
    public virtual void ShootBullet(
        float spread,
        float force,
        float damage,
        float bulletSize,
        int offset = 0
    )
    {
        Game.SetRandomSeed(Time.Tick + offset);

        var ray = Owner.AimRay;
        ShootBullet(ray.Position, ray.Forward, spread, force, damage, bulletSize);
    }

    [ClientRpc]
    public void CreateViewModel()
    {
        if (ViewModelPath == null)
        {
            Log.Info("View model path is null??");
            return;
        }
        else
        {
            Log.Info("View model path is not??");
        }

        ViewModelEntity = new WeaponViewModel
        {
            Position = Position,
            Owner = Owner,
            EnableViewmodelRendering = true
        };

        ViewModelEntity.Model = IsViewmodelCloud
            ? Cloud.Model(ViewModelPath)
            : Model.Load(ViewModelPath);
        //var vm = new WeaponViewModel( this );
        //vm.Model = Model.Load( ViewModelPath );
        //ViewModelEntity = vm;
    }

    [ClientRpc]
    public void DestroyViewModel()
    {
        if (ViewModelEntity.IsValid())
        {
            Log.Info("ViewModelEntity IS valid");
            ViewModelEntity.Delete();
        }
        else
        {
            Log.Info("ViewModelEntity is not valid");
        }
    }

    [ClientRpc]
    public virtual void playAnimation(
        string viewmodelAnimation = "reload",
        string worldmodelAnimation = "b_reload"
    )
    {
        Game.AssertClient();

        //Log.Info( $"Attempting to play viewmodel animation {viewmodelAnimation}" );
        //Log.Info( $"Does viemodel entity exist? {ViewModelEntity != null}" );

        Pawn.SetAnimParameter(worldmodelAnimation, true);
        ViewModelEntity?.SetAnimParameter(viewmodelAnimation, true);
    }

    public static Prefab? GetWeaponFromPrefab(string prefabName)
    {
        var prefab = ResourceLibrary
            .GetAll<Prefab>()
            .FirstOrDefault(prefab => prefab.ResourceName.ToLower() == prefabName.ToLower());
        return prefab;
    }
}
