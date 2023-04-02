using Content.Server.GameTicking;
using Content.Shared.Damage;
using Content.Shared.Item;
using Robust.Server.Containers;
using Robust.Shared.Configuration;

namespace Content.Server._CombatRim.DeathBorder;

public sealed class DeathBorderSystem : EntitySystem
{
    // Dependencies
    [Dependency] private readonly DamageableSystem _damageSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;


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
            if (xform.MapID != _gameTicker.DefaultMap)
                continue;

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
