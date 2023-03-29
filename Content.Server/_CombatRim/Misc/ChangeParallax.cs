using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.Maps;
using Content.Shared.Damage;
using Content.Shared.Item;
using Content.Shared.Parallax;
using Robust.Server.Containers;
using Robust.Shared.Configuration;
using Robust.Shared.Map;

namespace Content.Server._CombatRim.Misc;

/// <summary>
/// So if you're here, you may be asking "Why?"
/// Well it's due to "centcomm"
/// </summary>
public sealed class ChangeParallaxSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(ChangeParallax);
    }

    private void ChangeParallax(RoundStartingEvent args)
    {
        EnsureComp<ParallaxComponent>(_mapManager.GetMapEntityId(_gameTicker.DefaultMap)).Parallax = "Hell";
    }
}
