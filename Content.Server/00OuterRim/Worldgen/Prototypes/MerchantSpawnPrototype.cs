using Content.Server._00OuterRim.Worldgen.MerchantGeneration;
using Robust.Shared.Prototypes;

namespace Content.Server._00OuterRim.Worldgen.Prototypes;

/// <summary>
/// This is a prototype for...
/// </summary>
[Prototype("merchantSpawner")]
public sealed class MerchantSpawnPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField("generator", required: true)]
    public MerchantGenerator Generator { get; } = default!;
}
