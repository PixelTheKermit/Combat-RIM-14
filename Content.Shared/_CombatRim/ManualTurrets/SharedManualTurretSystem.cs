
using Content.Shared.Interaction.Events;
using Robust.Shared.Enums;
using Robust.Shared.Players;
using Robust.Shared.Serialization;

namespace Content.Shared._CombatRim.ManualTurret
{
    public abstract class SharedManualTurretSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ManualTurretComponent, InteractionAttemptEvent>(OnInteractAttempt);
            SubscribeLocalEvent<ManualTurretComponent, ChangeDirectionAttemptEvent>(OnChangeDirectionAttempt);
        }

        private void OnInteractAttempt(EntityUid uid, ManualTurretComponent component, InteractionAttemptEvent args)
        {
            args.Cancel();
        }

        private void OnChangeDirectionAttempt(EntityUid uid, ManualTurretComponent component, ChangeDirectionAttemptEvent args)
        {
            args.Cancel();
        }
    }

    [Serializable, NetSerializable]
    public sealed class TurretRotateEvent : EntityEventArgs
    {
        public EntityUid Uid;
        public Angle NewRot;

        public TurretRotateEvent(EntityUid uid, Angle newRot)
        {
            Uid = uid;
            NewRot = newRot;
        }
    }

    [Serializable, NetSerializable]
    public sealed class TurretAttemptShootEvent : EntityEventArgs
    {
        public EntityUid Uid;

        public TurretAttemptShootEvent(EntityUid uid)
        {
            Uid = uid;
        }
    }
}
