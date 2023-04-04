
using System.Diagnostics;
using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Events;
using Content.Server.Store.Components;
using Content.Server.Store.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.GameTicking;
using Content.Shared.Store;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CombatRim.Economy
{
    /// <summary>
    /// This will be the basis of the economy rework of CR.
    /// TODO: A fuckton
    /// </summary>
    public sealed class EconomySystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;
        [Dependency] private readonly IPrototypeManager _protoManager = default!;
        [Dependency] private readonly StoreSystem _storeSystem = default!;
        [Dependency] private readonly TransformSystem _transformSystem = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RoundStartingEvent>(RoundStarted);
            SubscribeLocalEvent<StoreComponent, ComponentInit>(StoreCompStartup);
            SubscribeLocalEvent<EcoContributorComponent, StoreBuyListingMessage>(StoreBuyListing);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            if (!EntityQuery<EconomyComponent>().Any())
                return;

            var comp = EntityQuery<EconomyComponent>().First();

            var curTime = _gameTiming.CurTime;

            if (curTime > comp.nextEconomicCrisis)
            {
                comp.nextEconomicCrisis = _gameTiming.CurTime + TimeSpan.FromMinutes(_random.Next(_cfg.GetCVar<int>(CombatRimCVars.EcoEventMinInterval), _cfg.GetCVar<int>(CombatRimCVars.EcoEventMaxInterval)));
                if (_cfg.GetCVar<bool>(CombatRimCVars.DoEcoEvents))
                    DoRandomEvent();
            }
        }

        private void StoreCompStartup(EntityUid uid, StoreComponent comp, ComponentInit args)
        {
            var ecoComp = GetEconomyComponent();

            if (ecoComp == null)
                return;

            var mainCur = _cfg.GetCVar<string>(CombatRimCVars.MainCurrency);

            foreach (var listing in comp.Listings)
            {
                if (listing.Cost.Any(x => x.Key == mainCur))
                {
                    listing.Cost[mainCur] *= ecoComp.inflationMultiplier;
                }
            }
        }

        private void StoreBuyListing(EntityUid uid, EcoContributorComponent contribComp, StoreBuyListingMessage msg)
        {
            var mainCur = _cfg.GetCVar<string>(CombatRimCVars.MainCurrency);

            if (!TryComp<StoreComponent>(uid, out var comp))
                return;

            var ecoComp = GetEconomyComponent();

            if (ecoComp == null)
                return;

            var listing = comp.Listings.FirstOrDefault(x => x.Equals(msg.Listing));

            if (msg.Session.AttachedEntity is not { Valid: true } buyer)
                return;

            if (!_storeSystem.ValidPurchase(uid, comp, buyer, listing))
                return;

            if (listing!.Cost.Any(x => x.Key == mainCur))
            {
                ecoComp.credits += (int) listing.Cost[mainCur];
            }
        }

        private void RoundStarted(RoundStartingEvent args)
        {
            var ent = Spawn(null, MapCoordinates.Nullspace);
            var comp = _entityManager.AddComponent<EconomyComponent>(ent);
            comp.credits = _random.Next(1, 3)*10000;
            Logger.Info("Credits: " + comp.credits);
            comp.nextEconomicCrisis = _gameTiming.CurTime + TimeSpan.FromMinutes(_random.Next(_cfg.GetCVar<int>(CombatRimCVars.EcoEventMinInterval), _cfg.GetCVar<int>(CombatRimCVars.EcoEventMaxInterval)));
            comp.inflationMultiplier = 1f;
        }

        public void DoRandomEvent()
        {
            var allEvents = _protoManager.EnumeratePrototypes<EconomicEventPrototype>().ToArray();

            if (!allEvents.Any())
                return;

            var ecoComp = GetEconomyComponent();

            if (ecoComp == null)
                return;

            var randEvent = _random.Pick(allEvents);

            ecoComp.credits += randEvent.AddedOn;
            ecoComp.credits *= randEvent.Multiplier;

            ecoComp.credits = (int) ecoComp.credits;

            if (ecoComp.credits < 0)
                ecoComp.credits = 0;

            EconomicInflation(ecoComp, randEvent.InflationMultiplier);

            _chatSystem.DispatchGlobalAnnouncement(Loc.GetString(randEvent.Text), _cfg.GetCVar<string>(CombatRimCVars.BankAnnouncer), true, null, Color.Violet);
        }

        private void EconomicInflation(EconomyComponent ecoComp, float multiplier)
        {
            var oldMultiplier = ecoComp.inflationMultiplier;

            ecoComp.inflationMultiplier *= multiplier;

            var mainCur = _cfg.GetCVar<string>(CombatRimCVars.MainCurrency);

            foreach (var comp in EntityQuery<StoreComponent>())
            {
                foreach (var listing in comp.Listings)
                {
                    if (listing.Cost.Any(x => x.Key == mainCur))
                    {
                        listing.Cost[mainCur] /= oldMultiplier;
                        listing.Cost[mainCur] *= ecoComp.inflationMultiplier;
                    }
                }
            }
        }
        public EconomyComponent? GetEconomyComponent()
        {
            if (!EntityQuery<EconomyComponent>().Any())
                return null;

            return EntityQuery<EconomyComponent>().First();
        }
    }
}
