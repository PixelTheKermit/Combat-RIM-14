using Content.Server.Cargo.Systems;
using Content.Server.MachineLinking.Events;
using Content.Server.Stack;
using Content.Shared.Examine;
using Content.Shared.Tag;
using Robust.Shared.Configuration;
using Robust.Server.GameObjects;
using Robust.Shared.Map;

namespace Content.Server._CombatRim.Economy
{
    public sealed class SellPlatformSystem : EntitySystem
    {
        [Dependency] private readonly TagSystem _tagSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly StackSystem _stackSystem = default!;
        [Dependency] private readonly EconomySystem _economySystem = default!;
        [Dependency] private readonly PricingSystem _pricingSystem = default!;
        [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
        [Dependency] private readonly TransformSystem _transformSystem = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SellPlatformComponent, SignalReceivedEvent>(OnSignal);
            SubscribeLocalEvent<SellPlatformComponent, ExaminedEvent>(OnInspect);
        }

        private void OnSignal(EntityUid uid, SellPlatformComponent comp, SignalReceivedEvent args)
        {
            if (args.Port == comp.SellPort)
            {
                if (!TryComp<TransformComponent>(uid, out var xform))
                    return;

                var ecoComp = _economySystem.GetEconomyComponent();

                if (ecoComp == null)
                    return;

                // No Static Flag, as we don't want players to just start selling walls. Sundries because otherwise you can't sell items on grids... but in space for some reason?
                var intersecting = _entityLookup.GetEntitiesIntersecting(uid, LookupFlags.Sundries | LookupFlags.Dynamic);

                var cash = 0;

                foreach (var otherUid in intersecting)
                {
                    if (otherUid == uid || (comp.Blacklist != null && comp.Blacklist.IsValid(otherUid)))
                        continue;

                    var cost = _pricingSystem.GetPrice(otherUid)*ecoComp.inflationMultiplier*(1-comp.TaxCut);

                    if (ecoComp.credits < cash + cost)
                        continue;

                    cash += (int) cost;
                    ecoComp.credits -= (int) cost;
                    _entityManager.QueueDeleteEntity(otherUid);
                }

                if (cash <= 0)
                    return;

                var spawned = Spawn(comp.MoneyPrototype, xform.Coordinates);
                _stackSystem.SetCount(spawned, cash);
            }
        }

        private void OnInspect(EntityUid uid, SellPlatformComponent comp, ExaminedEvent args)
        {
            var ecoComp = _economySystem.GetEconomyComponent();

            if (ecoComp == null)
                return;

            args.PushText(("Credits: " + ecoComp.credits));
        }
    }
}
