
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CombatRim.SoulTrapping
{
    [Serializable, NetSerializable]
    public sealed class SoulTrapDoAfterEvent : DoAfterEvent
    {
        public ItemSlot ItemSlot { get; }

        public SoulTrapDoAfterEvent(ItemSlot itemSlot)
        {
            ItemSlot = itemSlot;
        }

        public override DoAfterEvent Clone() => this;
    }
}
