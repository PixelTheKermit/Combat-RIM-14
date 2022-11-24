using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server._00OuterRim.Worldgen.Populators.Debris;

[ImplicitDataDefinitionForInheritors]
public abstract class DebrisPopulator
{
    public abstract void Populate(EntityUid gridEnt, MapGridComponent grid);
}
