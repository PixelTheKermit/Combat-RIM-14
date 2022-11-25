
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
    }
}
