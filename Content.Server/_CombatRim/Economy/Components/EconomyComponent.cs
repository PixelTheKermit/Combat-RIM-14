
using Content.Shared.Store;
using Content.Shared.Whitelist;

namespace Content.Server._CombatRim.Economy
{
    [RegisterComponent]
    public sealed class EconomyComponent : Component
    {
        public float Credits = 0f;
        public float InflationMultiplier = 1f;
        public TimeSpan? NextEconomicCrisis;

        public TimeSpan? NextStoreRefresh;
        public HashSet<ListingData> Listings = new();
    }
}
