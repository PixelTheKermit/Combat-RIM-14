namespace Content.Server._OuterRim.Generator;

/// <summary>
/// This is used for allowing you to insert fuel into gens.
/// </summary>
[RegisterComponent, Access(typeof(GeneratorSystem))]
public sealed class SolidFuelGeneratorAdapterComponent : Component
{
    [DataField("fuelMaterial"), ViewVariables(VVAccess.ReadWrite)]
    public string FuelMaterial = "Plasma";

    [DataField("multiplier")] public float Multiplier = 1.0f;
}
