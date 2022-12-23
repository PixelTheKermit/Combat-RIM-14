using Content.Server.ControllerDevice;
using Content.Server.Interaction;
using Content.Server.MobState;
using Content.Server.Power.Components;
using Content.Shared.Interaction;
using Content.Shared.MobState;
using Content.Shared.MobState.Components;
using Content.Shared.Popups;

namespace Content.Server.ControllableMob;

public sealed class ControllerStructureSystem : EntitySystem
{
    // Dependencies
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly ControllableMobSystem _controllableMobSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    public override void Initialize() // VERY IMPORTANT!!!!!!
    {
        base.Initialize();

        SubscribeLocalEvent<ControllerStructureComponent, InteractHandEvent>(Control);
        SubscribeLocalEvent<ControllerStructureComponent, InteractUsingEvent>(GetInteraction);
        SubscribeLocalEvent<ControllerStructureComponent, ComponentShutdown>(OnDeleted);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var (contStruct, apcReceiver, transform) in EntityQuery<ControllerStructureComponent, ApcPowerReceiverComponent, TransformComponent>())
        {
            // Checks to make sure that we are controlling an entity
            if (contStruct.Controlling == null || !_entityManager.EntityExists(contStruct.Controlling))
                continue; // REMINDER TO SELF, CONTINUE DOES NOT MEAN WHAT YOU THINK IT MEANS

            if (!TryComp<ControllableMobComponent>(contStruct.Controlling, out var controllableComp))
                continue;

            var owner = controllableComp.CurrentEntityOwning;

            if (owner == null || !TryComp<ControllerMobComponent>(owner.Value, out var controllerComp) || controllerComp.Controlling == null)
                continue;

            if (controllableComp.CurrentDeviceOwning == null || controllableComp.CurrentDeviceOwning.Value != contStruct.Owner)
                continue;

            // And now checks to ensure we are still able to control the entity
            if (apcReceiver.Powered &&
                (Comp<TransformComponent>(controllerComp.Owner).WorldPosition - transform.WorldPosition).Length <= InteractionSystem.InteractionRange*2)
                continue;

            controllerComp.Controlling = null;
            _controllableMobSystem.RevokeControl(owner.Value);
            controllableComp.CurrentEntityOwning = null;
        }
    }

    private void OnDeleted(EntityUid uid, ControllerStructureComponent comp, ComponentShutdown args)
    {
        // Checks to make sure that we are controlling an entity
        if (comp.Controlling == null || !_entityManager.EntityExists(comp.Controlling))
            return;

        if (!TryComp<ControllableMobComponent>(comp.Controlling, out var controllableComp))
            return;

        var owner = controllableComp.CurrentEntityOwning;

        if (owner == null || !TryComp<ControllerMobComponent>(owner.Value, out var controllerComp) || controllerComp.Controlling == null)
            return;

        if (controllableComp.CurrentDeviceOwning == null || controllableComp.CurrentDeviceOwning.Value != uid)
            return;

        controllerComp.Controlling = null;
        _controllableMobSystem.RevokeControl(owner.Value);
        controllableComp.CurrentEntityOwning = null;
    }

    private void Control(EntityUid uid, ControllerStructureComponent comp, InteractHandEvent args)
    {
        if (TryComp<ApcPowerReceiverComponent>(uid, out var apcReceiver) && !apcReceiver.Powered)
        {
            return;
        }

        if (comp.Controlling == null || !_entityManager.EntityExists(comp.Controlling) || (TryComp<MobStateComponent>(comp.Controlling, out var damageState) && damageState.CurrentState != null && damageState.CurrentState.Value == DamageState.Dead))
        {
            _popupSystem.PopupEntity(Loc.GetString("control-device-unable-to-connect"), uid, args.User);
            return;
        }

        if (!TryComp<ControllableMobComponent>(comp.Controlling, out var controllableComp)
            || controllableComp.CurrentEntityOwning != null)
        {
            _popupSystem.PopupEntity(Loc.GetString("control-device-already-controlled"), uid, args.User);
            return;
        }

        var calcDist = (Comp<TransformComponent>(uid).WorldPosition - Comp<TransformComponent>(comp.Controlling.Value).WorldPosition).Length;
        if (calcDist > comp.Range)
        {
            _popupSystem.PopupEntity(Loc.GetString("device-control-out-of-range"), uid, args.User);
            return;
        }

        if (!TryComp<ControllerMobComponent>(args.User, out var controllerComp)
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

    private void GetInteraction(EntityUid uid, ControllerStructureComponent comp, InteractUsingEvent args)
    {
        if (!TryComp<ControllerDeviceComponent>(args.Used, out var controlDeviceComp))
            return;

        if (comp.Controlling != null && TryComp<ControllableMobComponent>(comp.Controlling, out var controllableComp)
            && controllableComp.CurrentEntityOwning != null)
        {
            _popupSystem.PopupEntity(Loc.GetString("device-control-fail-pair-controlled"), uid, args.User);
            return;
        }

        comp.Controlling = controlDeviceComp.Controlling;
        _popupSystem.PopupEntity(Loc.GetString("device-control-paired"), uid, args.User);
    }
}
