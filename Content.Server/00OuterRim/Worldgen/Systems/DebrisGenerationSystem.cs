using System.Linq;
using Content.Server._00OuterRim.Worldgen.Components;
using Content.Server._00OuterRim.Worldgen.Prototypes;
using Content.Server.Shuttles.Systems;
using Content.Shared.Coordinates;
using Content.Shared.Shuttles.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server._00OuterRim.Worldgen.Systems;

public sealed class DebrisGenerationSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinition = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly ShuttleSystem _shuttleSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;

    public EntityUid GenerateDebris(DebrisPrototype proto, MapCoordinates location)
    {
        var (_, gridEnt) = GenerateFloorplan(proto, location);
        var unpop = AddComp<UnpopulatedComponent>(gridEnt);
        unpop.Populator = proto.Populator;
        EnsureComp<IFFComponent>(gridEnt);
        _shuttleSystem.SetIFFColor(gridEnt, proto.DebrisRadarColor);
        _shuttleSystem.AddIFFFlag(gridEnt, IFFFlags.HideLabel);
        return gridEnt;
    }

    /// <summary>
    /// Generates a new floorplan for the given debris.
    /// </summary>
    /// <returns></returns>
    public (IMapGrid, EntityUid) GenerateFloorplan(DebrisPrototype proto, MapCoordinates location)
    {
        var grid = _mapManager.CreateGrid(location.MapId);
        _transformSystem.SetWorldPosition(grid.GridEntityId, location.Position);

        switch (proto.FloorplanStyle)
        {
            case DebrisFloorplanStyle.Tiles:
                PlaceFloorplanTiles(proto, grid, false);
                break;
            case DebrisFloorplanStyle.Blobs:
                PlaceFloorplanTiles(proto, grid, true);
                break;
            default:
                throw new NotImplementedException();
        }

        return (grid, grid.GridEntityId);
    }

    private void PlaceFloorplanTiles(DebrisPrototype proto, IMapGrid grid, bool blobs)
    {
        // NO MORE THAN TWO ALLOCATIONS THANK YOU VERY MUCH.
        var spawnPoints = new HashSet<Vector2i>((int)proto.FloorPlacements * 4);
        var taken = new Dictionary<Vector2i, Tile>((int)proto.FloorPlacements);

        void PlaceTile(Vector2i point)
        {
            // Assume we already know that the spawn point is safe.
            spawnPoints.Remove(point);
            var north = point.Offset(Direction.North);
            var south = point.Offset(Direction.South);
            var east = point.Offset(Direction.East);
            var west = point.Offset(Direction.West);
            var radsq = Math.Pow(proto.Radius, 2); // I'd put this outside but i'm not 100% certain caching it between calls is a gain.

            // The math done is essentially a fancy way of comparing the distance from 0,0 to the radius,
            // and skipping the sqrt normally needed for dist.
            if (!taken.ContainsKey(north) && Math.Pow(north.X, 2) + Math.Pow(north.Y, 2)  <= radsq)
                spawnPoints.Add(north);
            if (!taken.ContainsKey(south) && Math.Pow(south.X, 2) + Math.Pow(south.Y, 2)  <= radsq)
                spawnPoints.Add(south);
            if (!taken.ContainsKey(east) && Math.Pow(east.X, 2) + Math.Pow(east.Y, 2)  <= radsq)
                spawnPoints.Add(east);
            if (!taken.ContainsKey(west) && Math.Pow(west.X, 2) + Math.Pow(west.Y, 2)  <= radsq)
                spawnPoints.Add(west);

            taken.Add(point, new Tile(_tileDefinition[_random.Pick(proto.FloorTiles)].TileId));

        }

        PlaceTile(Vector2i.Zero);

        for (var i = 0; i < proto.FloorPlacements; i++)
        {
            var point = _random.Pick(spawnPoints);
            PlaceTile(point);

            if (blobs)
            {
                if (!taken.ContainsKey(point.Offset(Direction.North)) && _random.Prob(0.5f))
                    PlaceTile(point.Offset(Direction.North));
                if (!taken.ContainsKey(point.Offset(Direction.South)) && _random.Prob(0.5f))
                    PlaceTile(point.Offset(Direction.South));
                if (!taken.ContainsKey(point.Offset(Direction.East)) && _random.Prob(0.5f))
                    PlaceTile(point.Offset(Direction.East));
                if (!taken.ContainsKey(point.Offset(Direction.West)) && _random.Prob(0.5f))
                    PlaceTile(point.Offset(Direction.West));
            }
        }

        grid.SetTiles(taken.Select(x => (x.Key, x.Value)).ToList());
    }


}
