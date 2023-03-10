
using Content.Server._CombatRim.ControllableMob.Components;
using Content.Server.DoAfter;
using Content.Server.Mind.Components;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Server.Containers;
using Robust.Shared.Configuration;
using System.Threading;

namespace Content.Server._CombatRim.ControllableMob;

public sealed class DeathBorderSystem : EntitySystem
{
    // Dependencies
    [Dependency] private readonly DamageableSystem _damageSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;


    List<(EntityUid, float)> queued = new();
    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var maxDist = _cfg.GetCVar<int>(CombatRimCVars.SectorBorderDist)*128;

        var e = EntityQueryEnumerator<DamageableComponent, TransformComponent>();

        DamageSpecifier damage = new();

        damage.DamageDict.Add("Blunt", 1f);
        damage.DamageDict.Add("Structural", 1f);

        while (e.MoveNext(out var uid, out var damageable, out var xform))
        {
            var distFrom0 = _transformSystem.GetWorldPosition(xform).Length;
            if (distFrom0 > maxDist)
            {
                queued.Add((uid, distFrom0-maxDist));
            }
        }

        foreach (var (uid, dmgMult) in queued)
        {
            if (!(HasComp<ItemComponent>(uid) && _containerSystem.IsEntityInContainer(uid)))
                _damageSystem.TryChangeDamage(uid, damage*dmgMult*frameTime);
        }

        queued.Clear();
    }


}

//Damageable
