using Content.Server._CombatRim.Control.Components;
using Content.Server.Interaction;
using Content.Server.Power.Components;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs.Components;
using Content.Server.Mind.Components;

namespace Content.Server._CombatRim.Control;

public sealed class ControllerStructureSystem : EntitySystem
{
    // Dependencies
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly ControllableSystem _controllableMobSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    public override void Initialize() // VERY IMPORTANT!!!!!!
    {
        base.Initialize();

        SubscribeLocalEvent<ControlStructureComponent, InteractHandEvent>(Control);
        SubscribeLocalEvent<ControlStructureComponent, InteractUsingEvent>(GetInteraction);
        SubscribeLocalEvent<ControlStructureComponent, ComponentShutdown>(OnDeleted);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var (contStruct, apcReceiver, transform) in EntityQuery<ControlStructureComponent, ApcPowerReceiverComponent, TransformComponent>())
        {
            // Checks to make sure that we are controlling an entity
            if (contStruct.Controlling == null || !_entityManager.EntityExists(contStruct.Controlling))
                continue; // REMINDER TO SELF, CONTINUE DOES NOT MEAN WHAT YOU THINK IT MEANS

            if (!TryComp<ControllableComponent>(contStruct.Controlling, out var controllableComp))
                continue;

            var owner = controllableComp.CurrentEntityOwning;

            if (owner == null || !TryComp<CanControlComponent>(owner.Value, out var controllerComp) || controllerComp.Controlling == null)
                continue;

            if (controllableComp.CurrentDeviceOwning == null || controllableComp.CurrentDeviceOwning.Value != contStruct.Owner)
                continue;

            // And now checks to ensure we are still able to control the entity
            if (apcReceiver.Powered &&
                (_transformSystem.GetWorldPosition(owner.Value) - _transformSystem.GetWorldPosition(transform)).Length <= SharedInteractionSystem.InteractionRange * 2)
                continue;

            controllerComp.Controlling = null;
            _controllableMobSystem.RevokeControl(owner.Value);
            controllableComp.CurrentEntityOwning = null;
        }
    }

    private void OnDeleted(EntityUid uid, ControlStructureComponent comp, ComponentShutdown args)
    {
        // Checks to make sure that we are controlling an entity
        if (comp.Controlling == null || !_entityManager.EntityExists(comp.Controlling))
            return;

        if (!TryComp<ControllableComponent>(comp.Controlling, out var controllableComp))
            return;

        var owner = controllableComp.CurrentEntityOwning;

        if (owner == null || !TryComp<CanControlComponent>(owner.Value, out var controllerComp) || controllerComp.Controlling == null)
            return;

        if (controllableComp.CurrentDeviceOwning == null || controllableComp.CurrentDeviceOwning.Value != uid)
            return;

        controllerComp.Controlling = null;
        _controllableMobSystem.RevokeControl(owner.Value);
        controllableComp.CurrentEntityOwning = null;
    }

    private void Control(EntityUid uid, ControlStructureComponent comp, InteractHandEvent args)
    {
        if (TryComp<ApcPowerReceiverComponent>(uid, out var apcReceiver) && !apcReceiver.Powered)
        {
            return;
        }

        if (comp.Controlling == null || !_entityManager.EntityExists(comp.Controlling) || TryComp<MobThresholdsComponent>(comp.Controlling, out var damageState) && damageState.CurrentThresholdState == MobState.Dead)
        {
            _popupSystem.PopupEntity(Loc.GetString("control-device-unable-to-connect"), uid, args.User);
            return;
        }

        if (!TryComp<ControllableComponent>(comp.Controlling, out var controllableComp)
            || controllableComp.CurrentEntityOwning != null)
        {
            _popupSystem.PopupEntity(Loc.GetString("control-device-already-controlled"), uid, args.User);
            return;
        }

        var calcDist = (_transformSystem.GetWorldPosition(uid) - _transformSystem.GetWorldPosition(comp.Controlling.Value)).Length;
        if (calcDist > comp.Range)
        {
            _popupSystem.PopupEntity(Loc.GetString("device-control-out-of-range"), uid, args.User);
            return;
        }

        if (!TryComp<CanControlComponent>(args.User, out var controllerComp)
            || controllerComp.Controlling != null)
        {
            _popupSystem.PopupEntity(Loc.GetString("control-device-unable-to-use"), uid, args.User);
            return;
        }

        controllableComp.CurrentDeviceOwning = uid;
        controllableComp.Range = comp.Range;
        controllerComp.Controlling = comp.Controlling;
        controllableComp.CurrentEntityOwning = args.User;
        _controllableMobSystem.GiveControl(controllableComp.CurrentEntityOwning.Value, comp.Controlling.Value);
    }

    private void GetInteraction(EntityUid uid, ControlStructureComponent comp, InteractUsingEvent args)
    {
        if (!TryComp<RemotePairerComponent>(args.Used, out var pairerComp))
            return;

        if (comp.Controlling != null && TryComp<ControllableComponent>(comp.Controlling, out var controllableComp)
            && controllableComp.CurrentEntityOwning != null)
        {
            _popupSystem.PopupEntity(Loc.GetString("device-control-fail-pair-controlled"), uid, args.User);
            return;
        }

        comp.Controlling = pairerComp.Entity;
        _popupSystem.PopupEntity(Loc.GetString("device-control-paired"), uid, args.User);
    }
}
