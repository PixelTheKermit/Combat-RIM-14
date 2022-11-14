using Content.Server._00OuterRim.Worldgen.Systems.Overworld;
using Content.Server._00OuterRim.Worldgen.Tools;
using Content.Server.Shuttles.Systems;
using Content.Shared.Shuttles.Components;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server._00OuterRim.Worldgen.PointOfInterest;

public sealed class ScatteredDebrisPoI : PointOfInterestGenerator
{
    [DataField("maps")]
    public List<string> Maps = default!;

    public override void Generate(Vector2i chunk)
    {
        var sampler = IoCManager.Resolve<PoissonDiskSampler>();
        var random = IoCManager.Resolve<IRobustRandom>();
        var mapLoader = IoCManager.Resolve<MapLoaderSystem>();
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var worldChunkSys = entityManager.EntitySysManager.GetEntitySystem<WorldChunkSystem>();
        var iffSys = entityManager.EntitySysManager.GetEntitySystem<ShuttleSystem>();

        var density = worldChunkSys.GetChunkDensity(chunk);
        var offs = (int)((WorldChunkSystem.ChunkSize - (density / 2)) / 2);
        var center = chunk * WorldChunkSystem.ChunkSize;
        var topLeft = (-offs, -offs);
        var lowerRight = (offs, offs);
        var debrisPoints = sampler.SampleRectangle(topLeft, lowerRight, density);

        foreach (var point in debrisPoints)
        {
            var _ = mapLoader.TryLoad(worldChunkSys.WorldMap, random.Pick(Maps), out var grid, new MapLoadOptions()
            {
                Offset = center + point,
                Rotation = random.NextAngle()
            });
            if (_)
                iffSys.AddIFFFlag(grid!.FirstOrDefault(), IFFFlags.HideLabel);

        }


    }
}
