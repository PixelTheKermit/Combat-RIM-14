using Content.Server._00OuterRim.Worldgen.Systems.Overworld;
using Content.Server._00OuterRim.Worldgen.Tools;
using Content.Server.Shuttles.Systems;
using Content.Shared.Shuttles.Components;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Random;
using System.Linq;
using static Robust.Shared.Physics.DynamicTree;

namespace Content.Server._00OuterRim.Worldgen.MerchantGeneration;

public sealed class ScatteredMerchants : MerchantGenerator
{

    [DataField("maps")]
    public List<string> Maps = default!;

    public override void Generate(Vector2i chunk)
    {
        var sampler = IoCManager.Resolve<PoissonDiskSampler>();
        var random = IoCManager.Resolve<IRobustRandom>();
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var mapLoader = entityManager.System<MapLoaderSystem>();
        var worldChunkSys = entityManager.EntitySysManager.GetEntitySystem<WorldChunkSystem>();
        var iffSys = entityManager.EntitySysManager.GetEntitySystem<ShuttleSystem>();

        var density = worldChunkSys.GetChunkDensity(chunk);
        var offs = (int) ((WorldChunkSystem.ChunkSize - (density / 2)) / 2);
        var center = chunk * WorldChunkSystem.ChunkSize;
        var topLeft = (-offs, -offs);
        var lowerRight = (offs, offs);
        var debrisPoints = sampler.SampleRectangle(topLeft, lowerRight, density);
        if (debrisPoints.Count > 0)
        {
            var point = debrisPoints[0];
            mapLoader.TryLoad(worldChunkSys.WorldMap, random.Pick(Maps), out var grid, new MapLoadOptions()
            {
                Offset = center + point,
                Rotation = random.NextAngle()
            });
            iffSys.AddIFFFlag(grid!.FirstOrDefault(), IFFFlags.HideLabel);
            iffSys.SetIFFColor(grid!.FirstOrDefault(), Color.ForestGreen);
        }


    }
}
