
using Content.Shared.Whitelist;

namespace Content.Server._CombatRim.Economy
{
    [RegisterComponent]
    public sealed class EconomyComponent : Component
    {
        public float credits = 0f;
        public float inflationMultiplier = 1f;
        public TimeSpan? nextEconomicCrisis;
    }
}
