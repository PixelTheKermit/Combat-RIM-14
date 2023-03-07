
using Content.Shared.Whitelist;

namespace Content.Server._CombatRim.Economy
{
    [RegisterComponent]
    public sealed class EconomyComponent : Component
    {
        public float credits = 0f;
        public float inflationMultiplier = 1f;
        public String currencyId = "SpaceCash"; // ! Should probably be moved to be a cvar, TODO: this later
        public TimeSpan? nextEconomicCrisis;
        public string announcer = "Bank of the Death Sector";
    }
}
