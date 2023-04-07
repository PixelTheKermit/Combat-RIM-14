using Content.Shared._CombatRim.ManualTurret;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Input;
using Robust.Shared.Timing;

namespace Content.Client._CombatRim.ManualTurret // TODO: Prediciton
{
    public sealed class ManualTurretSystem : SharedManualTurretSystem
    {
        [Dependency] private readonly InputSystem _inputSystem = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly TransformSystem _xformSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void FrameUpdate(float frameTime)
        {
            base.Update(frameTime);

            foreach (var (comp, xform) in EntityQuery<ManualTurretComponent, TransformComponent>())
            {
                if (_playerManager.LocalPlayer == null || _playerManager.LocalPlayer.ControlledEntity == null)
                    continue;

                if (comp.Owner != _playerManager.LocalPlayer.ControlledEntity)
                    continue;

                var input = _inputSystem.CmdStates;

                if (input.GetState(EngineKeyFunctions.MoveLeft) == BoundKeyState.Down)
                {
                    RaiseNetworkEvent(new TurretRotateEvent(_playerManager.LocalPlayer.ControlledEntity.Value, RotateType.anticlock, comp.Rotation));
                }
                else if (input.GetState(EngineKeyFunctions.MoveRight) == BoundKeyState.Down)
                {
                    RaiseNetworkEvent(new TurretRotateEvent(_playerManager.LocalPlayer.ControlledEntity.Value, RotateType.clock, comp.Rotation));
                }
                else
                {
                    RaiseNetworkEvent(new TurretRotateEvent(_playerManager.LocalPlayer.ControlledEntity.Value, RotateType.none, comp.Rotation));
                }
            }
        }
    }
}
