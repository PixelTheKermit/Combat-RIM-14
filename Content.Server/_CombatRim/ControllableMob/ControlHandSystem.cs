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
using Content.Server._CombatRim.Control.Components;

namespace Content.Server._CombatRim.Control
{
    public sealed class ControlHandSystem : EntitySystem
    {
        // Dependencies
        [Dependency] private readonly EntityManager _entityManager = default!;
        [Dependency] private readonly ControllableSystem _controllableSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

        public override void Initialize() // VERY IMPORTANT!!!!!!
        {
            base.Initialize();

            SubscribeLocalEvent<ControlHandComponent, UseInHandEvent>(Control);
            SubscribeLocalEvent<ControlHandComponent, GotUnequippedHandEvent>(Unequipped);
        }

        private void Control(EntityUid uid, ControlHandComponent comp, UseInHandEvent args)
        {
            if (!TryComp<RemotePairerComponent>(uid, out var pairComp))
                return;

            if (pairComp.Entity == null || !_entityManager.EntityExists(pairComp.Entity) || TryComp<MobThresholdsComponent>(pairComp.Entity, out var damageState) && damageState.CurrentThresholdState == MobState.Dead)
            {
                _popupSystem.PopupEntity(Loc.GetString("remote-unable-to-connect"), uid, args.User);
                return;
            }

            if (!TryComp<ControllableComponent>(pairComp.Entity, out var controllableComp)
                || TryComp<MindContainerComponent>(uid, out var mind) && mind.HasMind)
            {
                _popupSystem.PopupEntity(Loc.GetString("remote-already-controlled"), uid, args.User);
                return;
            }

            var calcDist = (_transformSystem.GetWorldPosition(uid) - _transformSystem.GetWorldPosition(pairComp.Entity.Value)).Length;
            if (calcDist > comp.Range)
            {
                _popupSystem.PopupEntity(Loc.GetString("device-control-out-of-range"), uid, args.User);
                return;
            }

            if (!TryComp<CanControlComponent>(args.User, out var controllerComp))
            {
                _popupSystem.PopupEntity(Loc.GetString("control-device-unable-to-use"), uid, args.User);
                return;
            }

            controllableComp.CurrentDeviceOwning = uid;
            controllableComp.Range = comp.Range;
            controllerComp.Controlling = pairComp.Entity;
            controllableComp.CurrentEntityOwning = args.User;
            _controllableSystem.GiveControl(controllableComp.CurrentEntityOwning.Value, pairComp.Entity.Value);
        }

        private void Unequipped(EntityUid uid, ControlHandComponent comp, GotUnequippedHandEvent args)
        {
            if (!TryComp<RemotePairerComponent>(uid, out var pairComp))
                return;

            // Checks to make sure that we are controlling an entity
            if (pairComp.Entity == null || !_entityManager.EntityExists(pairComp.Entity))
                return;

            if (!TryComp<ControllableComponent>(pairComp.Entity, out var controllableComp))
                return;

            var owner = controllableComp.CurrentEntityOwning;

            if (owner == null || !TryComp<CanControlComponent>(owner.Value, out var controllerComp) || controllerComp.Controlling == null)
                return;

            if (controllableComp.CurrentDeviceOwning == null || controllableComp.CurrentDeviceOwning.Value != uid)
                return;

            _controllableSystem.RevokeControl(args.User);
            controllerComp.Controlling = null;
            controllableComp.CurrentEntityOwning = null;
        }
    }

}
