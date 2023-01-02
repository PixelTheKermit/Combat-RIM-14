﻿using System.Linq;
using Content.Server._Citadel.Worldgen.Components.GC;
using Content.Server._Citadel.Worldgen.Prototypes;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Citadel.Worldgen.Systems.GC;

/// <summary>
/// This handles delayed garbage collection of entities, to avoid overloading the tick in particularly expensive cases.
/// </summary>
public sealed class GCQueueSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    [ViewVariables]
    private Dictionary<string, Queue<EntityUid>> _queues = new();
    [ViewVariables]
    private TimeSpan _maximumProcessTime = TimeSpan.Zero;

    /// <inheritdoc/>
    public override void Initialize()
    {
        _cfg.OnValueChanged(WorldgenCVars.GCMaximumTimeMs, s => _maximumProcessTime = TimeSpan.FromMilliseconds(s), true);
    }

    /// <inheritdoc/>
    public override void Update(float frameTime)
    {
        var overallWatch = new Stopwatch();
        var queueWatch = new Stopwatch();
        var queues = _queues.ToList();
        _random.Shuffle(queues); // Avert resource starvation by always processing in random order.
        overallWatch.Start();
        foreach (var (pId, queue) in queues)
        {
            if (overallWatch.Elapsed < _maximumProcessTime)
                return;

            var proto = _proto.Index<GCQueuePrototype>(pId);
            if (queue.Count <= proto.MinDepthToProcess)
                continue;

            queueWatch.Restart();
            while (queueWatch.Elapsed < proto.MaximumTickTime && queue.Count > proto.MinDepthToProcess && queue.Count != 0 && overallWatch.Elapsed < _maximumProcessTime)
            {
                var e = queue.Dequeue();
                if (!Deleted(e))
                    Del(e);
            }
        }
    }

    /// <summary>
    /// Attempts to GC an entity. This functions as QueueDel if it can't.
    /// </summary>
    /// <param name="e">Entity to GC.</param>
    public void TryGCEntity(EntityUid e)
    {
        if (!TryComp<GCAbleObjectComponent>(e, out var comp))
        {
            QueueDel(e); // not our problem :)
            return;
        }

        if (!_queues.TryGetValue(comp.Queue, out var queue))
        {
            queue = new Queue<EntityUid>();
            _queues[comp.Queue] = queue;
        }

        var proto = _proto.Index<GCQueuePrototype>(comp.Queue);
        if (queue.Count > proto.Depth)
        {
            QueueDel(e); // whelp, too full.
            return;
        }

        if (proto.TrySkipQueue)
        {
            var ev = new TryGCImmediately();
            RaiseLocalEvent(e, ref ev);
            if (!ev.Cancelled)
            {
                QueueDel(e);
                return;
            }
        }

        queue.Enqueue(e);
    }
}

/// <summary>
/// Fired by GCQueueSystem to check if it can simply immediately GC an entity, for example if it was never fully loaded.
/// </summary>
/// <param name="Cancelled">Whether or not the immediate deletion attempt was cancelled.</param>
[ByRefEvent]
public record struct TryGCImmediately(bool Cancelled = false);
