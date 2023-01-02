﻿using Content.Server._Citadel.Worldgen.Components.Carvers;
using Content.Server._Citadel.Worldgen.Systems.Debris;

namespace Content.Server._Citadel.Worldgen.Systems.Carvers;

/// <summary>
/// This handles...
/// </summary>
public sealed class NoiseRangeCarverSystem : EntitySystem
{
    [Dependency] private readonly NoiseIndexSystem _index = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<NoiseRangeCarverComponent, PrePlaceDebrisFeatureEvent>(OnPrePlaceDebris);
    }

    private void OnPrePlaceDebris(EntityUid uid, NoiseRangeCarverComponent component, ref PrePlaceDebrisFeatureEvent args)
    {
        var coords = WorldGen.WorldToChunkCoords(args.Coords.ToMapPos(EntityManager));
        var val = _index.Evaluate(uid, component.NoiseChannel, coords);

        foreach (var (low, high) in component.Ranges)
        {
            if (low > val || high < val)
                continue;

            args.Cancelled = true;
            return;
        }
    }
}
