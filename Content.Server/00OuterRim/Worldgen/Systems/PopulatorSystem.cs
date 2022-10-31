using System.Linq;
using Content.Server._00OuterRim.Worldgen.Components;
using Content.Server.Ghost.Components;
using Robust.Server.Player;
using Robust.Shared.Timing;

namespace Content.Server._00OuterRim.Worldgen.Systems;

public class PopulatorSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Update(float frameTime)
    {

        foreach (var (unpop, grid, transform) in EntityManager.EntityQuery<UnpopulatedComponent, IMapGridComponent, TransformComponent>())
        {
            // TODO: This could be much smarter with high player counts or large debris, but it's fine for now.
            bool nearby = false;

            foreach (var session in _playerManager.ServerSessions)
            {
                if (session.AttachedEntity is not null &&
                    (session.AttachedEntityTransform?.WorldPosition - transform.WorldPosition)?.Length <
                    64 && // Must be withing 64 units of the unpopulated debris.
                    !HasComp<GhostComponent>(session.AttachedEntity.Value))
                {
                    nearby = true;
                    break;
                }
            }

            if (nearby)
            {
                var startTime = _gameTiming.RealTime;
                unpop.Populator?.Populate(grid.Owner, grid.Grid);
                var timeSpan = _gameTiming.RealTime - startTime;
                Logger.InfoS("worldgen", $"Populated grid {grid.Owner} in {timeSpan.TotalMilliseconds:N2}ms.");
                RemComp<UnpopulatedComponent>(unpop.Owner);
            }

        }
    }
}
