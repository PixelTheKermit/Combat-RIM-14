using Content.Server.MachineLinking.Components;
using Content.Shared.Audio;
using Content.Shared.Interaction;
using Content.Shared.MachineLinking;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Server.MachineLinking.System
{
    public sealed class ButtonedLeverSystem : EntitySystem
    {
        [Dependency] private readonly SignalLinkerSystem _signalSystem = default!;

        const string _leftToggleImage = "rotate_ccw.svg.192dpi.png";
        const string _rightToggleImage = "rotate_cw.svg.192dpi.png";

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<ButtonedLeverComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<ButtonedLeverComponent, InteractHandEvent>(OnActivated);
            SubscribeLocalEvent<ButtonedLeverComponent, ActivateInWorldEvent>(OnGetInteraction);
            SubscribeLocalEvent<ButtonedLeverComponent, GetVerbsEvent<InteractionVerb>>(OnGetInteractionVerbs);
            SubscribeLocalEvent<ButtonedLeverComponent, GetVerbsEvent<AlternativeVerb>>(OnGetInteractionAltVerbs);
        }

        private void OnInit(EntityUid uid, ButtonedLeverComponent component, ComponentInit args)
        {
            _signalSystem.EnsureTransmitterPorts(uid, component.LeftPort, component.RightPort, component.MiddlePort);
        }

        private void OnActivated(EntityUid uid, ButtonedLeverComponent component, InteractHandEvent args)
        {
            if (args.Handled)
                return;

            component.Pressed = !component.Pressed;
            _signalSystem.InvokePort(uid, component.Pressed ? component.OnPort : component.OffPort);
            SoundSystem.Play(component.ClickSound.GetSound(), Filter.Pvs(component.Owner), component.Owner,
                AudioHelpers.WithVariation(0.125f).WithVolume(8f));

            args.Handled = true;
        }

        private void OnGetInteraction(EntityUid uid, ButtonedLeverComponent component, ActivateInWorldEvent args)
        {
            if (args.Handled || component.State == TwoWayLeverState.Left)
                return;

            component.State = component.State switch
            {
                TwoWayLeverState.Middle => TwoWayLeverState.Left,
                TwoWayLeverState.Right => TwoWayLeverState.Middle,
                _ => throw new ArgumentOutOfRangeException()
            };
            StateChanged(uid, component);

            args.Handled = true;
        }

        private void OnGetInteractionVerbs(EntityUid uid, ButtonedLeverComponent component, GetVerbsEvent<InteractionVerb> args)
        {
            if (!args.CanAccess || !args.CanInteract || (args.Hands == null))
                return;

            InteractionVerb verbLeft = new()
            {
                Act = () =>
                {
                    component.State = component.State switch
                    {
                        TwoWayLeverState.Middle => TwoWayLeverState.Left,
                        TwoWayLeverState.Right => TwoWayLeverState.Middle,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    StateChanged(uid, component);
                },
                Message = Loc.GetString("two-way-lever-cant"),
                Disabled = component.State == TwoWayLeverState.Left,
                IconTexture = $"/Textures/Interface/VerbIcons/{_leftToggleImage}",
                Text = Loc.GetString("two-way-lever-left"),
            };

            args.Verbs.Add(verbLeft);
        }

        private void OnGetInteractionAltVerbs(EntityUid uid, ButtonedLeverComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanAccess || !args.CanInteract || (args.Hands == null))
                return;

            AlternativeVerb verbRight = new()
            {
                Act = () =>
                {
                    component.State = component.State switch
                    {
                        TwoWayLeverState.Left => TwoWayLeverState.Middle,
                        TwoWayLeverState.Middle => TwoWayLeverState.Right,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    StateChanged(uid, component);
                },
                Message = Loc.GetString("two-way-lever-cant"),
                Disabled = component.State == TwoWayLeverState.Right,
                IconTexture = $"/Textures/Interface/VerbIcons/{_rightToggleImage}",
                Text = Loc.GetString("two-way-lever-right"),
            };

            args.Verbs.Add(verbRight);
        }

        private void StateChanged(EntityUid uid, ButtonedLeverComponent component)
        {
            if (component.State == TwoWayLeverState.Middle)
                component.NextSignalLeft = !component.NextSignalLeft;

            if (TryComp(uid, out AppearanceComponent? appearanceComponent))
                appearanceComponent.SetData(TwoWayLeverVisuals.State, component.State);

            var port = component.State switch
            {
                TwoWayLeverState.Left => component.LeftPort,
                TwoWayLeverState.Right => component.RightPort,
                TwoWayLeverState.Middle => component.MiddlePort,
                _ => throw new ArgumentOutOfRangeException()
            };

            _signalSystem.InvokePort(uid, port);
        }
    }
}
