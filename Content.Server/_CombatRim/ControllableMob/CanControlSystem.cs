using Content.Server._CombatRim.Control.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;

namespace Content.Server._CombatRim.Control;

public sealed class CanControlSystem : EntitySystem
{
    // Dependencies
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly ControllableSystem _contMobSystem = default!;
    public override void Initialize() // VERY IMPORTANT!!!!!!
    {
        base.Initialize();

        SubscribeLocalEvent<CanControlComponent, DamageChangedEvent>(WasHurt);
        //SubscribeLocalEvent<CanControlComponent, ComponentShutdown>(OnDeleted);
        // TODO: make the player a ghost if this entity was deleted
    }

    private void OnDeleted(EntityUid uid, CanControlComponent comp, ComponentShutdown args)
    {
        if (comp.Controlling != null && _entityManager.EntityExists(comp.Controlling)
            && TryComp<ControllableComponent>(comp.Controlling, out var controllableComp))
        {
            _contMobSystem.RevokeControl(uid);
            controllableComp.CurrentEntityOwning = null;
        }
    }

    private void WasHurt(EntityUid uid, CanControlComponent comp, DamageChangedEvent args)
    {
        if (comp.Controlling != null && _entityManager.EntityExists(comp.Controlling)
            && TryComp<ControllableComponent>(comp.Controlling, out var controllableComp)
            && args.DamageIncreased)
        {
            _contMobSystem.RevokeControl(uid);
            comp.Controlling = null;
            controllableComp.CurrentEntityOwning = null;
        }
    }
}
