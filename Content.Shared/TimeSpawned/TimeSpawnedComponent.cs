using Robust.Shared.GameStates;

namespace Content.Shared.TimeSpawned;

[RegisterComponent, NetworkedComponent]
public sealed class TimeSpawnedComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan TimeSpawned = TimeSpan.Zero;
}
