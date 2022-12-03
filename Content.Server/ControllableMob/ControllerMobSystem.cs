
using Content.Server.Mind.Components;
using Content.Server.Players;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Verbs;
using Robust.Server.Player;

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
    }

    private void WasHurt(EntityUid uid, ControllerMobComponent comp, DamageChangedEvent args)
    {
        if (comp.Controlling != null && _entityManager.EntityExists(comp.Controlling)
            && TryComp<ControllableMobComponent>(comp.Controlling, out var controllableComp))
        {
            _contMobSystem.ChangeControl(comp.Controlling.Value, uid);
            comp.Controlling = null;
            controllableComp.CurrentEntityOwning = null;
        }
    }
}
