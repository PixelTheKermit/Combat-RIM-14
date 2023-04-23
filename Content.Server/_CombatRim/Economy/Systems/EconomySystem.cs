
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
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RoundStartingEvent>(RoundStarted);
            SubscribeLocalEvent<RestockableComponent, ComponentInit>(StoreCompStartup);
            SubscribeLocalEvent<EcoContributorComponent, StoreBuyListingMessage>(StoreBuyListing);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            if (!EntityQuery<EconomyComponent>().Any())
                return;

            var comp = EntityQuery<EconomyComponent>().First();

            var curTime = _gameTiming.CurTime;

            if (curTime > comp.NextEconomicCrisis)
            {
                comp.NextEconomicCrisis = _gameTiming.CurTime + TimeSpan.FromMinutes(_random.Next(_cfg.GetCVar<int>(CombatRimCVars.EcoEventMinInterval), _cfg.GetCVar<int>(CombatRimCVars.EcoEventMaxInterval)));
                if (_cfg.GetCVar<bool>(CombatRimCVars.DoEcoEvents))
                    DoRandomEvent();
            }
            if (curTime > comp.NextStoreRefresh)
            {
                comp.NextEconomicCrisis = _gameTiming.CurTime + TimeSpan.FromMinutes(_random.Next(_cfg.GetCVar<int>(CombatRimCVars.NextRestockMinInterval), _cfg.GetCVar<int>(CombatRimCVars.NextRestockMaxInterval)));
            }
        }

        private void StoreCompStartup(EntityUid uid, RestockableComponent comp, ComponentInit args)
        {
            ReloadStore(uid);
        }

        private void StoreBuyListing(EntityUid uid, EcoContributorComponent contribComp, StoreBuyListingMessage msg)
        {
            var mainCur = _cfg.GetCVar<string>(CombatRimCVars.MainCurrency);

            if (!TryComp<StoreComponent>(uid, out var comp))
                return;

            if (!EntityQuery<EconomyComponent>().Any())
                return;

            var ecoComp = EntityQuery<EconomyComponent>().First();

            var listing = comp.Listings.FirstOrDefault(x => x.Equals(msg.Listing));

            if (msg.Session.AttachedEntity is not { Valid: true } buyer)
                return;

            if (!_storeSystem.ValidPurchase(uid, comp, buyer, listing))
                return;

            if (listing!.Cost.Any(x => x.Key == mainCur))
            {
                ecoComp.Credits += (int) listing.Cost[mainCur];
            }
        }

        private void RoundStarted(RoundStartingEvent args)
        {
            var ent = Spawn(null, MapCoordinates.Nullspace);
            var comp = _entityManager.AddComponent<EconomyComponent>(ent);
            comp.Credits = _random.Next(1, 3)*10000;
            Logger.Info("Credits: " + comp.Credits);
            comp.NextEconomicCrisis = _gameTiming.CurTime + TimeSpan.FromMinutes(_random.Next(_cfg.GetCVar<int>(CombatRimCVars.EcoEventMinInterval), _cfg.GetCVar<int>(CombatRimCVars.EcoEventMaxInterval)));
            comp.InflationMultiplier = 1f;
            RefreshStores();
        }

        public void DoRandomEvent()
        {
            var allEvents = _protoManager.EnumeratePrototypes<EconomicEventPrototype>().ToArray();

            if (!allEvents.Any())
                return;

            if (!EntityQuery<EconomyComponent>().Any())
                return;

            var ecoComp = EntityQuery<EconomyComponent>().First();

            var randEvent = _random.Pick(allEvents);

            ecoComp.Credits += randEvent.AddedOn;
            ecoComp.Credits *= randEvent.Multiplier;

            ecoComp.Credits = (int) ecoComp.Credits;

            if (ecoComp.Credits < 0)
                ecoComp.Credits = 0;

            EconomicInflation(ecoComp, randEvent.InflationMultiplier);

            _chatSystem.DispatchGlobalAnnouncement(Loc.GetString(randEvent.Text), _cfg.GetCVar<string>(CombatRimCVars.BankAnnouncer), true, null, Color.Violet);
        }

        /// <summary>
        /// Changes the current stock of items.
        /// </summary>
        private void RefreshStores()
        {
            if (!EntityQuery<EconomyComponent>().Any())
                return;

            var eco = EntityQuery<EconomyComponent>().First();

            eco.Listings.Clear();

            foreach (var listing in _protoManager.EnumeratePrototypes<RandListingPrototype>())
            {
                var listingProto = _protoManager.EnumeratePrototypes<ListingPrototype>().FirstOrDefault(x => x.Name == listing.ListingID);

                if (listingProto == null)
                {
                    Logger.Error(listing.ListingID + " does not exist as an actual listing!");
                    continue;
                }

                if (_random.Prob(listing.ListingChance))
                {
                    eco.Listings.Add((ListingData) listingProto.Clone());
                }
            }

            foreach (var (_, store) in EntityQuery<RestockableComponent, StoreComponent>())
            {
                ReloadStore(store);
            }
        }

        /// <summary>
        /// Refreshes the store's stock with the new one.
        /// </summary>
        /// <param name="uid">The uid of the entity, can be replaced with a StoreComponent</param>
        private void ReloadStore(EntityUid uid)
        {
            if (!TryComp<StoreComponent>(uid, out var comp))
                return;

            ReloadStore(comp);
        }

        /// <summary>
        /// Refreshes the store's stock with the new one.
        /// </summary>
        /// <param name="uid">Can be replaced with the uid of the entity</param>
        private void ReloadStore(StoreComponent comp)
        {
            if (!EntityQuery<EconomyComponent>().Any())
                return;

            var eco = EntityQuery<EconomyComponent>().First();

            comp.Listings.Clear();

            comp.Listings = eco.Listings;

            var mainCur = _cfg.GetCVar<string>(CombatRimCVars.MainCurrency);

            foreach (var listing in comp.Listings)
            {
                if (listing.Cost.Any(x => x.Key == mainCur))
                    listing.Cost[mainCur] *= eco.InflationMultiplier;
            }
        }

        private void EconomicInflation(EconomyComponent ecoComp, float multiplier)
        {
            ecoComp.InflationMultiplier *= multiplier;

            foreach (var comp in EntityQuery<StoreComponent>())
            {
                ReloadStore(comp);
            }
        }
    }
}
