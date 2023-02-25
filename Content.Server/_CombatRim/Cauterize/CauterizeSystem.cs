using Content.Server.DoAfter;
using Content.Server.Light.Components;
using Content.Shared.Damage;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.Player;
using Content.Shared.Smoking;
using System.Threading;
using Content.Shared.Interaction;

namespace Content.Server._CombatRim.Cauterize
{
    public sealed class CauterizeSystem : EntitySystem // ! This likely won't be needed or will be broken as soon as wounds become a thing.
    {
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<CauterizeComponent, AfterInteractEvent>(OnInteract);
            SubscribeLocalEvent<CauterizeComponent, CauterizeComplete>(OnCauterizeCompleted);
            SubscribeLocalEvent<CauterizeComponent, CauterizeCancelledEvent>(OnCauterizeCancelled);
        }

        private void OnInteract(EntityUid uid, CauterizeComponent component, AfterInteractEvent args) // TODO: Make this work with other forms of flamable objects (like lighters, but probably not welders (yeowch))
        {
            if (component.CancelToken != null || args.Target == null || !TryComp<MatchstickComponent>(uid, out var matchComp))
                return;

            if (!args.Handled && matchComp.CurrentState == SmokableState.Lit)
            {
                _popupSystem.PopupEntity("You try to seal the wounds", args.User, args.User); // TODO: Localisations
                component.CancelToken = new CancellationTokenSource();
                _doAfterSystem.DoAfter(new DoAfterEventArgs(uid, component.Delay, component.CancelToken.Token, args.User)
                {
                    UsedFinishedEvent = new CauterizeComplete(uid, args.User, args.Target.Value),
                    UsedCancelledEvent = new CauterizeCancelledEvent(),
                    BreakOnTargetMove = true,
                    BreakOnUserMove = true,
                    BreakOnStun = true,
                });
            }
        }

        private void OnCauterizeCompleted(EntityUid uid, CauterizeComponent component, CauterizeComplete args)
        {
            component.CancelToken = null;

            _popupSystem.PopupEntity("You feel a sharp pain where the wound once was!", args.Target, args.Target); // TODO: Localisations
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
            public EntityUid Target { get; }

            public CauterizeComplete(EntityUid used, EntityUid user, EntityUid target)
            {
                Used = used;
                User = user;
                Target = target;
            }
        }

        private sealed class CauterizeCancelledEvent : EntityEventArgs { }
    }
}
