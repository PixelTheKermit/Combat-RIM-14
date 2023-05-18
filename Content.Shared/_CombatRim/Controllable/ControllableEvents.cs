using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CombatRim.Controllable
{
    [Serializable, NetSerializable]
    public sealed class RemotePairerDoAfterEvent : DoAfterEvent
    {
        public RemotePairerDoAfterEvent()
        {
        }

        public override DoAfterEvent Clone() => this;
    }
}
