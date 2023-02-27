using Content.Server._CombatRim.ControllableMob.Components;
using Content.Server.DoAfter;
using Content.Server.Mind.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using System.Threading;

namespace Content.Server._CombatRim.ControllableMob;

public sealed class ControllableMobSystem : EntitySystem
{
    // Dependencies
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;

    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    public override void Initialize() // VERY IMPORTANT!!!!!!
    {
        base.Initialize();

        SubscribeLocalEvent<ControllableMobComponent, GetVerbsEvent<ActivationVerb>>(AddActVerb);
        SubscribeLocalEvent<ControllableMobComponent, MobStateChangedEvent>(MobStateChanged);
        SubscribeLocalEvent<ControllableMobComponent, ComponentShutdown>(OnDeleted);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var (contMob, xform) in EntityQuery<ControllableMobComponent, TransformComponent>())
        {
            if (contMob.CurrentEntityOwning != null)
            {
                var calcDist = (_transformSystem.GetWorldPosition(contMob.CurrentEntityOwning.Value) - _transformSystem.GetWorldPosition(xform)).Length;

                if (calcDist > contMob.Range)
                {
                    RevokeControl(contMob.CurrentEntityOwning.Value);
                    Comp<ControllerMobComponent>(contMob.CurrentEntityOwning.Value).Controlling = null;
                    _popupSystem.PopupEntity(Loc.GetString("device-control-out-of-range"), contMob.CurrentEntityOwning.Value, contMob.CurrentEntityOwning.Value);
                    contMob.CurrentDeviceOwning = null;
                    contMob.CurrentEntityOwning = null;
                }
            }
        }
    }

    /// <summary>
    /// Give temporary control to an entity.
    /// </summary>
    /// <param name="former">The original entity.</param>
    /// <param name="latter">The entity that you want to temporarily control.</param>
    public void GiveControl(EntityUid former, EntityUid latter)
    {
        if (!_entityManager.TryGetComponent<MindComponent>(former, out var mindComp))
            return;

        var mind = mindComp.Mind;

        if (mind == null)
            return;

        _entityManager.RemoveComponent<VisitingMindComponent>(latter);


        mind.Visit(latter);
    }

    /// <summary>
    /// Revoke control of the entity.
    /// </summary>
    /// <param name="uid">The entity that is controlling.</param>
    public void RevokeControl(EntityUid uid)
    {
        if (!_entityManager.TryGetComponent<MindComponent>(uid, out var mindComp))
            return;

        var mind = mindComp.Mind;

        if (mind == null)
            return;

        mind.UnVisit();

    }

    private void OnDeleted(EntityUid uid, ControllableMobComponent comp, ComponentShutdown args)
    {
        if (comp.CurrentEntityOwning != null &&
            TryComp<ControllerMobComponent>(comp.CurrentEntityOwning, out var controlComp))
        {
            controlComp.Controlling = null;

            RevokeControl(comp.CurrentEntityOwning.Value);

            comp.CurrentEntityOwning = null;
        }
    }

    private void MobStateChanged(EntityUid uid, ControllableMobComponent comp, MobStateChangedEvent args)
    {
        if (comp.CurrentEntityOwning != null && args.NewMobState == MobState.Dead &&
            TryComp<ControllerMobComponent>(comp.CurrentEntityOwning, out var controlComp))
        {
            controlComp.Controlling = null;

            RevokeControl(comp.CurrentEntityOwning.Value);

            comp.CurrentEntityOwning = null;
        }
    }

    private void StopControl(EntityUid uid, ControllableMobComponent comp)
    {
        if (comp.CurrentEntityOwning != null && TryComp<ControllerMobComponent>(comp.CurrentEntityOwning, out var controlComp))
        {
            controlComp.Controlling = null;

            RevokeControl(comp.CurrentEntityOwning.Value);

            comp.CurrentEntityOwning = null;
        }
    }

    private void AddActVerb(EntityUid uid, ControllableMobComponent comp, GetVerbsEvent<ActivationVerb> args)
    {
        if (args.User != uid || !_entityManager.EntityExists(uid))
            return;

        ActivationVerb verb = new()
        {
            Act = () =>
            {
                StopControl(uid, comp);
            },
            Text = "Stop controlling",
            Priority = 1
        };

        args.Verbs.Add(verb);
    }
}
