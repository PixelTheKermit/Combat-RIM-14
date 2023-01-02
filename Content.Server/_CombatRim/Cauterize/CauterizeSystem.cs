using Content.Server.DoAfter;
using Content.Server.Light.Components;
using Content.Shared.Damage;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.Player;
using Content.Shared.Smoking;
using Content.Server.Nutrition.EntitySystems;
using System.Threading;

namespace Content.Server._CombatRim.Cauterize
{
    public sealed class CauterizeSystem : EntitySystem
    {
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<CauterizeComponent, UseInHandEvent>(OnInteract);
            SubscribeLocalEvent<CauterizeComponent, CauterizeComplete>(OnCauterizeCompleted);
            SubscribeLocalEvent<CauterizeComponent, CauterizeCancelledEvent>(OnCauterizeCancelled);
        }

        private void OnInteract(EntityUid uid, CauterizeComponent component, UseInHandEvent args)
        {
            if (!TryComp<MatchstickComponent>(uid, out var matchComp))
                return;
                
            if (!args.Handled && matchComp.CurrentState == SmokableState.Lit)
            {
                _popupSystem.PopupEntity("You try to painfully seal your wounds!", args.User, args.User);
                component.CancelToken = new CancellationTokenSource();
                _doAfterSystem.DoAfter(new DoAfterEventArgs(uid, component.Delay, component.CancelToken.Token, args.User)
                {
                    UserFinishedEvent = new CauterizeComplete(uid, args.User),
                    UserCancelledEvent = new CauterizeCancelledEvent(),
                    BreakOnTargetMove = true,
                    BreakOnUserMove = true,
                    BreakOnStun = true,
                });
            }
        }

        private void OnCauterizeCompleted(EntityUid uid, CauterizeComponent component, CauterizeComplete args)
        {
            component.CancelToken = null;

            _damageableSystem.TryChangeDamage(args.User, component.LitCauterizeDamage);
            _audioSystem.Play("/Audio/Effects/lightburn.ogg", Filter.Pvs(args.User), args.User, true);
        }

        private void OnCauterizeCancelled(EntityUid uid, CauterizeComponent component, CauterizeCancelledEvent args)
        {
            component.CancelToken = null;
        }

        private sealed class CauterizeComplete : EntityEventArgs
        {
            public EntityUid Used { get; }
            public EntityUid User { get; }

            public CauterizeComplete(EntityUid used, EntityUid user)
            {
                Used = used;
                User = user;
            }
        }

        private sealed class CauterizeCancelledEvent : EntityEventArgs { }
    }
}
