using Content.Shared._CombatRim.ManualTurret;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Input;
using Robust.Shared.Timing;

namespace Content.Client._CombatRim.ManualTurret // TODO: Prediciton
{
    public sealed class ManualTurretSystem : SharedManualTurretSystem
    {
        [Dependency] private readonly InputSystem _inputSystem = default!;
        [Dependency] private readonly IInputManager _inputManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly TransformSystem _xformSystem = default!;
        [Dependency] private readonly IEyeManager _eyeManager = default!;

        // Time in seconds to wait until updating input to the server
        private const float Delay = 1f / 10; // 10 Hz
        private float _timePassed; // Time passed since last update sent to the server.
        public override void Initialize()
        {
            base.Initialize();
            UpdatesOutsidePrediction = true;
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            if (!_gameTiming.IsFirstTimePredicted)
                return;

            if (_playerManager.LocalPlayer == null || _playerManager.LocalPlayer.ControlledEntity == null)
                return;

            var uid = _playerManager.LocalPlayer!.ControlledEntity!.Value;

            if (!TryComp<ManualTurretComponent>(uid, out var comp))
                return;

            _timePassed += frameTime;

            if (_timePassed < Delay)
                return;

            _timePassed = 0;


            var newRotation = (_eyeManager.ScreenToMap(_inputManager.MouseScreenPosition).Position - _xformSystem.GetWorldPosition(uid)).ToAngle() + Angle.FromDegrees(90);

            RaiseNetworkEvent(new TurretRotateEvent(uid, newRotation));
        }

    }
}
