using Content.Shared.Damage;

namespace Content.Server.ControllableMob;

public sealed class ControllerMobSystem : EntitySystem
{
    // Dependencies
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly ControllableMobSystem _contMobSystem = default!;
    public override void Initialize() // VERY IMPORTANT!!!!!!
    {
        base.Initialize();

        SubscribeLocalEvent<ControllerMobComponent, DamageChangedEvent>(WasHurt);
        //SubscribeLocalEvent<ControllerMobComponent, ComponentShutdown>(OnDeleted); TODO: make the player a ghost if this entity was deleted
    }

    private void OnDeleted(EntityUid uid, ControllerMobComponent comp, ComponentShutdown args)
    {
        if (comp.Controlling != null && _entityManager.EntityExists(comp.Controlling)
            && TryComp<ControllableMobComponent>(comp.Controlling, out var controllableComp))
        {
            _contMobSystem.RevokeControl(uid);
            controllableComp.CurrentEntityOwning = null;
        }
    }

    private void WasHurt(EntityUid uid, ControllerMobComponent comp, DamageChangedEvent args)
    {
        if (comp.Controlling != null && _entityManager.EntityExists(comp.Controlling)
            && TryComp<ControllableMobComponent>(comp.Controlling, out var controllableComp))
        {
            _contMobSystem.RevokeControl(uid);
            comp.Controlling = null;
            controllableComp.CurrentEntityOwning = null;
        }
    }
}
