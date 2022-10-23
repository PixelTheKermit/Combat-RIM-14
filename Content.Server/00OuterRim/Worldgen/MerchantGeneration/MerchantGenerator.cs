namespace Content.Server._00OuterRim.Worldgen.MerchantGeneration;

[ImplicitDataDefinitionForInheritors]
public abstract class MerchantGenerator
{
    public abstract void Generate(Vector2i chunk);
}
