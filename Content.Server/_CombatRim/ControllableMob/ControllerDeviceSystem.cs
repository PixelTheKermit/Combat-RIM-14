using Content.Server._CombatRim.ControllableMob.Components;
using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Interaction;
using Content.Server.DoAfter;
using System.Threading;
using Content.Shared.DoAfter;
using Content.Server.Mind;
using Content.Server.Mind.Components;

namespace Content.Server._CombatRim.ControllableMob;

public sealed class ControllerDeviceSystem : EntitySystem
{
    // Dependencies
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly ControllableMobSystem _controllableMobSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;

    public override void Initialize() // VERY IMPORTANT!!!!!!
    {
        base.Initialize();

        SubscribeLocalEvent<ControllerDeviceComponent, UseInHandEvent>(Control);
        SubscribeLocalEvent<ControllerDeviceComponent, GotUnequippedHandEvent>(Unequipped);
        SubscribeLocalEvent<ControllerDeviceComponent, AfterInteractEvent>(GetInteraction);
        SubscribeLocalEvent<ControllerDeviceComponent, DoAfterEvent>(OnDoAfter);
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

    private void Control(EntityUid uid, ControllerDeviceComponent comp, UseInHandEvent args) //TODO: Fix a crash that occurs with ghosting (keep this vague so that no one knows what it is)
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

        var calcDist = (_transformSystem.GetWorldPosition(uid) - _transformSystem.GetWorldPosition(comp.Controlling.Value)).Length;
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

    private void GetInteraction(EntityUid uid, ControllerDeviceComponent comp, AfterInteractEvent args)
    {

        if (comp.CancelToken != null || args.Target == null || args.User == null || !TryComp<ControllableMobComponent>(args.Target, out var mobComp))
            return;

        if (mobComp.CurrentEntityOwning != null)
        {
            _popupSystem.PopupEntity(Loc.GetString("device-control-fail-pair-controlled"), uid, args.User);
            return;
        }

        if (TryComp<MobThresholdsComponent>(uid, out var damageState) && _mobStateSystem.IsDead(uid))
        {
            _popupSystem.PopupEntity(Loc.GetString("device-control-fail-pair-damaged"), uid, args.User);
            return;
        }

        comp.CancelToken = new CancellationTokenSource();

        var eventArgs = new DoAfterEventArgs(args.User, mobComp.Delay*comp.Multiplier, comp.CancelToken.Token, args.Target, uid)
        {
            BreakOnDamage = true,
            BreakOnTargetMove = true,
            BreakOnUserMove = true,
            BreakOnStun = true,
            NeedHand = true,
        };

        _doAfterSystem.DoAfter(eventArgs);
    }

    private void OnDoAfter(EntityUid uid, ControllerDeviceComponent comp, DoAfterEvent args)
    {
        comp.CancelToken = null;

        if (args.Cancelled || args.Args.Target == null)
            return;

        comp.Controlling = args.Args.Target;
        _popupSystem.PopupEntity(Loc.GetString("device-control-paired"), uid, args.Args.User);
    }
}
