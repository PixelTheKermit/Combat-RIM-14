﻿using Content.Server._Citadel.Worldgen.Systems.Debris;

namespace Content.Server._Citadel.Worldgen.Components.Debris;

/// <summary>
/// This is used for attaching a piece of debris to it's owning controller.
/// Mostly just syncs deletion.
/// </summary>
[RegisterComponent, Access(typeof(DebrisFeaturePlacerSystem))]
public sealed class OwnedDebrisComponent : Component
{
    /// <summary>
    /// The DebrisFeaturePlacerController-having entity that owns this.
    /// </summary>
    [DataField("owningController")]
    public EntityUid OwningController;

    /// <summary>
    /// The last location in the controller's internal structure for this debris.
    /// </summary>
    [DataField("lastKey")]
    public Vector2 LastKey;
}
