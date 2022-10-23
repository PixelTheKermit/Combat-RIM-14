namespace Content.Server._00OuterRim.Worldgen.PointOfInterest;

[ImplicitDataDefinitionForInheritors]
public abstract class PointOfInterestGenerator
{
    public abstract void Generate(Vector2i chunk);
}
