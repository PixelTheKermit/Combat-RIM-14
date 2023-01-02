using Content.Shared.Construction.Prototypes;
using Content.Shared.MachineLinking;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._CombatRim.GasFly;

[RegisterComponent]
public sealed class GasFlyComponent : Component
{
    [DataField("gasRequired")]
    public float GasRequired = 42.5f;
}
