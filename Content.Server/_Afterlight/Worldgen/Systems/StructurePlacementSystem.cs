using System.Linq;
using Content.Server._Afterlight.Worldgen.Components;
using Content.Server._Citadel.Worldgen.Components;
using Content.Server._Citadel.Worldgen.Tools;
using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Server._Afterlight.Worldgen.Systems;

/// <summary>
/// This handles placing pre-designed structures into the world
/// </summary>
public sealed class StructurePlacementSystem : EntitySystem
{
    [Dependency] private readonly PoissonDiskSampler _sampler = default!;
    [Dependency] private readonly IAdminLogManager _log = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private ISawmill _sawmill = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<StructurePlacementComponent, MapInitEvent>(OnMapInit);
        _sawmill = _logManager.GetSawmill("worldgen.structures");
    }

    private void OnMapInit(EntityUid uid, StructurePlacementComponent component, MapInitEvent args)
    {
        if (!HasComp<WorldControllerComponent>(uid))
            return;

        var totalLocations = (int)(component.Structures.Sum(x => x.AmountRange.Y) * 2f);

        var locations = GetRandomValidPoints(uid, component, totalLocations);
        var mapId = Comp<MapComponent>(uid).WorldMap;
        var safetyBox = Box2.UnitCentered.Enlarged(component.SafetyRadius);

        foreach (var config in component.Structures)
        {
            var toPlace = _random.Next(config.AmountRange.X, config.AmountRange.Y);

            while (toPlace != 0)
            {
                var point = _random.PickAndTake(locations);
                var fail = _map.FindGridsIntersecting(mapId, safetyBox.Translated(point)).Any();
                if (fail)
                {
                    continue;
                }

                var ent = Spawn(config.Entity, new MapCoordinates(point, mapId));
                _log.Add(LogType.ALPlacedStructure, LogImpact.Medium, $"Spawned {ToPrettyString(ent)} at {new MapCoordinates(point, mapId)} for {ToPrettyString(uid)}.");
                toPlace--;
            }
        }
    }

    private List<Vector2> GetRandomValidPoints(EntityUid uid, StructurePlacementComponent component, int amount)
    {
        var points = _sampler.SampleCircle(Vector2.Zero, component.PlacementRadius, component.SafetyRadius);

        if (points.Count < amount)
        {
            _sawmill.Error($"Failed to place {amount} points as {ToPrettyString(uid)}, attempting to continue anyways!");
        }

        _sawmill.Info($"Placing {amount} debris at {points.Count} locations as {ToPrettyString(uid)}.");

        return points;
    }
}
