using System.Threading;
using Robust.Shared.Serialization;


namespace Content.Shared._CombatRim.SoulTrapping
{
    [RegisterComponent]
    public sealed class SoulTrapperComponent : Component
    {
        [DataField("capturingMultiplier")]
        public float Multiplier = 1f;

        public CancellationTokenSource? CancelToken;
    }

    [Serializable, NetSerializable]
    public enum SoulTrapperVisuals : byte
    {
        Inserted,
    }
}
