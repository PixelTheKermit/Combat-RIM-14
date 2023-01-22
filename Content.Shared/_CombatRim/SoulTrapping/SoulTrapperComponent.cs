using Robust.Shared.Serialization;


namespace Content.Shared._CombatRim.SoulTrapping.Components
{
    [RegisterComponent]
    public sealed class SoulTrapperComponent : Component
    {
        [DataField("capturingMultiplier")]
        public float Multiplier = 1f;
    }

    [Serializable, NetSerializable]
    public enum SoulTrapperVisuals : byte
    {
        Inserted,
    }
}
