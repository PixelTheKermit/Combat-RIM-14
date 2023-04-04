using Content.Server._CombatRim.Control.Components;
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

namespace Content.Server._CombatRim.Control;

public sealed class ControllableSystem : EntitySystem
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

        SubscribeLocalEvent<ControllableComponent, GetVerbsEvent<ActivationVerb>>(AddActVerb);
        SubscribeLocalEvent<ControllableComponent, MobStateChangedEvent>(MobStateChanged);
        SubscribeLocalEvent<ControllableComponent, ComponentShutdown>(OnDeleted);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var (contMob, xform) in EntityQuery<ControllableComponent, TransformComponent>())
        {
            if (contMob.CurrentEntityOwning != null)
            {
                var calcDist = (_transformSystem.GetWorldPosition(contMob.CurrentEntityOwning.Value) - _transformSystem.GetWorldPosition(xform)).Length;

                if (calcDist > contMob.Range)
                {
                    RevokeControl(contMob.CurrentEntityOwning.Value);
                    Comp<CanControlComponent>(contMob.CurrentEntityOwning.Value).Controlling = null;
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

        _entityManager.RemoveComponent<VisitingMindComponent>(latter); // ! This is nessessary to prevent crashes

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

    private void OnDeleted(EntityUid uid, ControllableComponent comp, ComponentShutdown args)
    {
        if (comp.CurrentEntityOwning != null &&
            TryComp<CanControlComponent>(comp.CurrentEntityOwning, out var controlComp))
        {
            controlComp.Controlling = null;

            RevokeControl(comp.CurrentEntityOwning.Value);

            comp.CurrentEntityOwning = null;
        }
    }

    private void MobStateChanged(EntityUid uid, ControllableComponent comp, MobStateChangedEvent args)
    {
        if (comp.CurrentEntityOwning != null && args.NewMobState == MobState.Dead &&
            TryComp<CanControlComponent>(comp.CurrentEntityOwning, out var controlComp))
        {
            controlComp.Controlling = null;

            RevokeControl(comp.CurrentEntityOwning.Value);

            comp.CurrentEntityOwning = null;
        }
    }

    private void StopControl(EntityUid uid, ControllableComponent comp)
    {
        if (comp.CurrentEntityOwning != null && TryComp<CanControlComponent>(comp.CurrentEntityOwning, out var controlComp))
        {
            controlComp.Controlling = null;

            RevokeControl(comp.CurrentEntityOwning.Value);

            comp.CurrentEntityOwning = null;
        }
    }

    private void AddActVerb(EntityUid uid, ControllableComponent comp, GetVerbsEvent<ActivationVerb> args)
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
