
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server.Merchant.Sell
{
    [RegisterComponent]
    public sealed class MerchantSellComponent : Component
    {
        [DataField("tax")]
        public float Tax = 0.8f;

        [DataField("funds")]
        public int Funds = 10000;

        [DataField("cashPrototype")]
        public string CashPrototype = "SpaceCash";

        [DataField("blacklist")]
        public List<string> Blacklist = new();
    }
}
