using Robust.Shared.Timing;

namespace Content.Shared.TimeSpawned;

public sealed class TimeSpawnedSystem : EntitySystem
{
    // Dependencies
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    public override void Initialize() // VERY IMPORTANT!!!!!!
    {
        base.Initialize();

        SubscribeLocalEvent<TimeSpawnedComponent, ComponentInit>(Init);
    }

    private void Init(EntityUid uid, TimeSpawnedComponent component, ComponentInit args)
    {
        component.TimeSpawned = _gameTiming.CurTime;
    }
}
