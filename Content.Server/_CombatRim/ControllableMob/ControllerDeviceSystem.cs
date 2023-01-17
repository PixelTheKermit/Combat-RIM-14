using Content.Server._CombatRim.ControllableMob.Components;
using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;

namespace Content.Server._CombatRim.ControllableMob;

public sealed class ControllerDeviceSystem : EntitySystem
{
    // Dependencies
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly ControllableMobSystem _controllableMobSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    public override void Initialize() // VERY IMPORTANT!!!!!!
    {
        base.Initialize();

        SubscribeLocalEvent<ControllerDeviceComponent, UseInHandEvent>(Control);
        SubscribeLocalEvent<ControllerDeviceComponent, GotUnequippedHandEvent>(Unequipped);
    }

    private void OnDeleted(EntityUid uid, ControllerMobComponent comp, ComponentShutdown args)
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
        _controllableMobSystem.RevokeControl(comp.Controlling.Value);
        controllableComp.CurrentEntityOwning = null;
    }

    private void Control(EntityUid uid, ControllerDeviceComponent comp, UseInHandEvent args)
    {
        if (!comp.Enabled)
            return;

        if (comp.Controlling == null || !_entityManager.EntityExists(comp.Controlling) || TryComp<MobThresholdsComponent>(comp.Controlling, out var damageState) && damageState.CurrentThresholdState == MobState.Dead)
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

    private void Unequipped(EntityUid uid, ControllerDeviceComponent comp, GotUnequippedHandEvent args)
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

        _controllableMobSystem.RevokeControl(args.User);
        controllerComp.Controlling = null;
        controllableComp.CurrentEntityOwning = null;
    }
}
