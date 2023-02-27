using Content.Server.DoAfter;
using Content.Server.Light.Components;
using Content.Shared.Damage;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.Player;
using Content.Shared.Smoking;
using System.Threading;
using Content.Shared.Interaction;
using Content.Shared.DoAfter;

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
            SubscribeLocalEvent<CauterizeComponent, DoAfterEvent>(OnDoAfter);
        }

        private void OnInteract(EntityUid uid, CauterizeComponent component, AfterInteractEvent args) // TODO: Make this work with other forms of flamable objects (like lighters, but probably not welders (yeowch))
        {
            if (component.CancelToken != null || args.Target == null || !TryComp<MatchstickComponent>(uid, out var matchComp))
                return;

            if (!args.Handled && matchComp.CurrentState == SmokableState.Lit)
            {
                _popupSystem.PopupEntity("You try to seal the wounds", args.User, args.User); // TODO: Localisations, I'm too lazy to do them now
                component.CancelToken = new CancellationTokenSource();

                var eventArgs = new DoAfterEventArgs(args.User, component.Delay, component.CancelToken.Token, uid, args.Target)
                {
                    BreakOnTargetMove = true,
                    BreakOnUserMove = true,
                    BreakOnStun = true,
                };

                _doAfterSystem.DoAfter(eventArgs);
            }
        }

        private void OnDoAfter(EntityUid uid, CauterizeComponent component, DoAfterEvent args)
        {
            component.CancelToken = null;

            if (args.Cancelled || args.Args.Target == null)
                return;

            _popupSystem.PopupEntity("You feel a sharp pain where the wound once was!", args.Args.Target.Value, args.Args.Target.Value); // TODO: Localisations
            _damageableSystem.TryChangeDamage(args.Args.User, component.LitCauterizeDamage);
            _audioSystem.Play("/Audio/Effects/lightburn.ogg", Filter.Pvs(args.Args.User), args.Args.User, true);
        }
    }
}
