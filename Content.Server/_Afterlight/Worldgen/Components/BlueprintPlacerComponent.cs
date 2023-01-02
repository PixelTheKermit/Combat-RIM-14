using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Utility;

namespace Content.Server._Afterlight.Worldgen.Components;

/// <summary>
/// This is used for simply spawning a blueprint into the world on init.
/// </summary>
[RegisterComponent]
public sealed class BlueprintPlacerComponent : Component
{
    [DataField("blueprint", required: true, customTypeSerializer: typeof(ResourcePathSerializer))]
    public ResourcePath Blueprint = default!;

    /// <summary>
    /// The components that get added to the target grid.
    /// </summary>
    [DataField("components", required: true)]
    public EntityPrototype.ComponentRegistry Components { get; } = default!;

    //TODO: Get someone to make this a method on componentregistry that does it Correctly.
    /// <summary>
    /// Applies the worldgen config to the given target (presumably a grid.)
    /// </summary>
    public void Apply(EntityUid target, ISerializationManager serialization, IEntityManager entityManager, IComponentFactory componentFactory)
    {
        // Add all components required by the prototype. Engine update for this whenst.
        foreach (var data in Components.Values)
        {
            var comp = (Component) serialization.CreateCopy(data.Component, notNullableOverride: true);
            comp.Owner = target;
            entityManager.AddComponent(target, comp);
        }
    }
}
