using Content.Server.Atmos.EntitySystems;
using Content.Server.Light.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Smoking;
using Content.Shared.Temperature;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using System.Threading;
using Content.Server.DoAfter;

namespace Content.Server.Light.EntitySystems
{
    public sealed class MatchstickSystem : EntitySystem
    {
        private HashSet<MatchstickComponent> _litMatches = new();
        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly TransformSystem _transformSystem = default!;
        [Dependency] private readonly SharedItemSystem _item = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MatchstickComponent, InteractUsingEvent>(OnInteractUsing);
            SubscribeLocalEvent<MatchstickComponent, UseInHandEvent>(OnInteract);
            SubscribeLocalEvent<MatchstickComponent, IsHotEvent>(OnIsHotEvent);
            SubscribeLocalEvent<MatchstickComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<MatchstickComponent, MeleeHitEvent>(OnMeleeHit);

            SubscribeLocalEvent<MatchstickComponent, CauterizeComplete>(OnCauterizeCompleted);
            SubscribeLocalEvent<MatchstickComponent, CauterizeCancelledEvent>(OnCauterizeCancelled);
        }

        private void OnInteract(EntityUid uid, MatchstickComponent component, UseInHandEvent args)
        {
            if (!args.Handled && component.CurrentState == SmokableState.Lit && component.CancelToken == null)
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

        private void OnCauterizeCompleted(EntityUid uid, MatchstickComponent component, CauterizeComplete args)
        {
            component.CancelToken = null;

            _damageableSystem.TryChangeDamage(args.User, component.LitMeleeDamageBonus);
            _audioSystem.Play("/Audio/Effects/lightburn.ogg", Filter.Pvs(args.User), args.User, true);
        }

        private void OnCauterizeCancelled(EntityUid uid, MatchstickComponent component, CauterizeCancelledEvent args)
        {
            component.CancelToken = null;
        }

        private void OnMeleeHit(EntityUid uid, MatchstickComponent component, MeleeHitEvent args)
        {
            if (!args.Handled && component.CurrentState == SmokableState.Lit)
            {
                args.BonusDamage += component.LitMeleeDamageBonus;
            }
        }

        private void OnShutdown(EntityUid uid, MatchstickComponent component, ComponentShutdown args)
        {
            _litMatches.Remove(component);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            foreach (var match in _litMatches)
            {
                if (match.CurrentState != SmokableState.Lit || Paused(match.Owner) || match.Deleted)
                    continue;

                var xform = Transform(match.Owner);

                if (xform.GridUid is not {} gridUid)
                    return;

                var position = _transformSystem.GetGridOrMapTilePosition(match.Owner, xform);

                _atmosphereSystem.HotspotExpose(gridUid, position, 400, 50, true);
            }
        }

        private void OnInteractUsing(EntityUid uid, MatchstickComponent component, InteractUsingEvent args)
        {
            if (args.Handled || component.CurrentState != SmokableState.Unlit)
                return;

            var isHotEvent = new IsHotEvent();
            RaiseLocalEvent(args.Used, isHotEvent, false);

            if (!isHotEvent.IsHot)
                return;

            Ignite(component, args.User);
            args.Handled = true;
        }

        private void OnIsHotEvent(EntityUid uid, MatchstickComponent component, IsHotEvent args)
        {
            args.IsHot = component.CurrentState == SmokableState.Lit;
        }

        public void Ignite(MatchstickComponent component, EntityUid user)
        {
            // Play Sound
            SoundSystem.Play(component.IgniteSound.GetSound(), Filter.Pvs(component.Owner),
                component.Owner, AudioHelpers.WithVariation(0.125f).WithVolume(-0.125f));

            // Change state
            SetState(component, SmokableState.Lit);
            _litMatches.Add(component);
            component.Owner.SpawnTimer(component.Duration * 1000, delegate
            {
                SetState(component, SmokableState.Burnt);
                _litMatches.Remove(component);
            });
        }

        private void SetState(MatchstickComponent component, SmokableState value)
        {
            component.CurrentState = value;

            if (TryComp<PointLightComponent>(component.Owner, out var pointLightComponent))
            {
                pointLightComponent.Enabled = component.CurrentState == SmokableState.Lit;
            }

            if (EntityManager.TryGetComponent(component.Owner, out ItemComponent? item))
            {
                switch (component.CurrentState)
                {
                    case SmokableState.Lit:
                        _item.SetHeldPrefix(component.Owner, "lit", item);
                        break;
                    default:
                        _item.SetHeldPrefix(component.Owner, "unlit", item);
                        break;
                }
            }

            if (EntityManager.TryGetComponent(component.Owner, out AppearanceComponent? appearance))
            {
                appearance.SetData(SmokingVisuals.Smoking, component.CurrentState);
            }
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
