
using Content.Server.ControllerDevice;
using Content.Server.Interaction;
using Content.Server.Mind.Components;
using Content.Server.MobState;
using Content.Server.Players;
using Content.Server.Power.Components;
using Content.Shared.Damage;
using Content.Shared.Hands;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.MobState;
using Content.Shared.MobState.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Server.Player;
using Robust.Shared.Localization;
using Robust.Shared.Player;

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
            if (contStruct.Controlling != null && contStruct.Controlling != null
                && _entityManager.EntityExists(contStruct.Controlling) && TryComp<ControllableMobComponent>(contStruct.Controlling, out var controllableComp) // TODO: REDUCE THE DAMN CHONK OF THIS IF STATEMENT
                && controllableComp.CurrentDeviceOwning == contStruct.Owner && controllableComp.CurrentEntityOwning != null && TryComp<ControllerMobComponent>(controllableComp.CurrentEntityOwning.Value, out var controllerComp)
                && controllerComp.Controlling != null && (!apcReceiver.Powered || (Comp<TransformComponent>(controllerComp.Owner).WorldPosition - transform.WorldPosition).Length > InteractionSystem.InteractionRange))
            {
                controllerComp.Controlling = null;
                _controllableMobSystem.RevokeControl(controllableComp.CurrentEntityOwning.Value);
                controllableComp.CurrentEntityOwning = null;
            }    
        }
    }

    private void OnDeleted(EntityUid uid, ControllerStructureComponent comp, ComponentShutdown args)
    {
        if (comp.Controlling != null && _entityManager.EntityExists(comp.Controlling) // CHONKY
            && TryComp<ControllableMobComponent>(comp.Controlling, out var controllableComp)
            && controllableComp.CurrentDeviceOwning == uid && controllableComp.CurrentEntityOwning != null
            && TryComp<ControllerMobComponent>(controllableComp.CurrentEntityOwning.Value, out var controllerComp)
            && controllerComp.Controlling != null)
        {
            controllerComp.Controlling = null;
            _controllableMobSystem.RevokeControl(controllableComp.CurrentEntityOwning.Value);
            controllableComp.CurrentEntityOwning = null;
        }
    }

    private void Control(EntityUid uid, ControllerStructureComponent comp, InteractHandEvent args)
    {
        if (TryComp<ApcPowerReceiverComponent>(uid, out var apcReceiver) && !apcReceiver.Powered)
        {
            return;
        }

        if (comp.Controlling == null || !_entityManager.EntityExists(comp.Controlling) || (TryComp<MobStateComponent>(comp.Controlling, out var damageState) && damageState.CurrentState != null && damageState.CurrentState.Value == DamageState.Dead))
        {
            _popupSystem.PopupEntity(Loc.GetString("control-device-unable-to-connect"), uid, Filter.Entities(args.User));
            return;
        }

        if (!TryComp<ControllableMobComponent>(comp.Controlling, out var controllableComp)
            || controllableComp.CurrentEntityOwning != null)
        {
            _popupSystem.PopupEntity(Loc.GetString("control-device-already-controlled"), uid, Filter.Entities(args.User));
            return;
        }

        var calcDist = (Comp<TransformComponent>(uid).WorldPosition - Comp<TransformComponent>(comp.Controlling.Value).WorldPosition).Length;
        if (calcDist > comp.Range)
        {
            _popupSystem.PopupEntity(Loc.GetString("device-control-out-of-range"), uid, Filter.Entities(args.User));
            return;
        }

        if (!TryComp<ControllerMobComponent>(args.User, out var controllerComp)
            || controllerComp.Controlling != null)
        {
            _popupSystem.PopupEntity(Loc.GetString("control-device-unable-to-use"), uid, Filter.Entities(args.User));
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
            _popupSystem.PopupEntity(Loc.GetString("control-device-already-controlled"), uid, Filter.Entities(args.User));
            return;
        }

        comp.Controlling = controlDeviceComp.Controlling;
        _popupSystem.PopupEntity(Loc.GetString("device-control-paired"), uid, Filter.Entities(args.User));
    }
}
