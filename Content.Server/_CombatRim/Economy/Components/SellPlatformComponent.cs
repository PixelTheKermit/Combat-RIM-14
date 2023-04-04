
using Content.Shared.Whitelist;

namespace Content.Server._CombatRim.Economy
{
    [RegisterComponent]
    public sealed class SellPlatformComponent : Component
    {
        [DataField("taxCut")]
        public float TaxCut = .2f;

        [DataField("blacklist")]
        public EntityWhitelist? Blacklist;

        [DataField("moneyPrototype")]
        public string MoneyPrototype = "SpaceCash";

        public HashSet<EntityUid> Contacts = new();

        public string SellPort = "Sell";
    }
}
