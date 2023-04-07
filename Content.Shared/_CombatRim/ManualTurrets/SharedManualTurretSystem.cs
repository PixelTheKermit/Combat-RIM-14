
using Robust.Shared.Enums;
using Robust.Shared.Players;
using Robust.Shared.Serialization;

namespace Content.Shared._CombatRim.ManualTurret
{
    public abstract class SharedManualTurretSystem : EntitySystem
    {
    }

    [Serializable, NetSerializable]
    public sealed class TurretRotateEvent : EntityEventArgs
    {
        public EntityUid Uid;
        public RotateType Rotation;
        public Angle ClientRot;

        public TurretRotateEvent(EntityUid uid, RotateType rotation, Angle clientRot)
        {
            Uid = uid;
            Rotation = rotation;
            ClientRot = clientRot;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ResetClientRotationEvent : EntityEventArgs
    {
        public EntityUid Uid;
        public Angle ServerRotation;

        public ResetClientRotationEvent(EntityUid uid, Angle rotation)
        {
            Uid = uid;
            ServerRotation = rotation;
        }
    }

    public enum RotateType : byte
    {
        clock = 0,
        anticlock = 1,
        none = 2
    }

    [Serializable, NetSerializable]
    public sealed class TurretShootEvent : EntityEventArgs {}
}
