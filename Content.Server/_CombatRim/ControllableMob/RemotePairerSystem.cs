
using Content.Shared.Interaction;
using Content.Server._CombatRim.Control.Components;
using Content.Shared.Mobs.Systems;
using Content.Server.DoAfter;
using Content.Shared.DoAfter;
using Content.Server.Mind;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Serialization;
using Content.Shared._CombatRim.Controllable;

namespace Content.Server._CombatRim.Control
{
    public sealed class RemotePairerSystem : EntitySystem
    {
        [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
        [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RemotePairerComponent, AfterInteractEvent>(GetInteraction);
            SubscribeLocalEvent<RemotePairerComponent, RemotePairerDoAfterEvent>(OnDoAfter);
        }

        private void GetInteraction(EntityUid uid, RemotePairerComponent comp, AfterInteractEvent args)
        {

            if (comp.CancelToken != null || args.Target == null || args.User == null || !TryComp<ControllableComponent>(args.Target, out var mobComp))
                return;

            if (mobComp.CurrentEntityOwning != null)
            {
                _popupSystem.PopupEntity(Loc.GetString("pairer-fail-controlled"), uid, args.User);
                return;
            }

            if (HasComp<MobThresholdsComponent>(uid) && _mobStateSystem.IsDead(uid))
            {
                _popupSystem.PopupEntity(Loc.GetString("pairer-fail-damaged"), uid, args.User);
                return;
            }

            if (comp.DevMode) // For mapping.
            {
                comp.Entity = args.Target;
                return;
            }

            var eventArgs = new DoAfterArgs(args.User, TimeSpan.FromSeconds(mobComp.Delay*comp.Multiplier), new RemotePairerDoAfterEvent(), uid, args.Target, uid)
            {
                BreakOnDamage = true,
                BreakOnTargetMove = true,
                BreakOnUserMove = true,
                CancelDuplicate = true,
                RequireCanInteract = true,
                NeedHand = true,
            };

            _doAfterSystem.TryStartDoAfter(eventArgs);
        }

        private void OnDoAfter(EntityUid uid, RemotePairerComponent comp, DoAfterEvent args)
        {
            if (args.Cancelled || args.Args.Target == null)
                return;

            comp.Entity = args.Args.Target;
            _popupSystem.PopupEntity(Loc.GetString("device-control-paired"), uid, args.Args.User);
        }
    }
}
