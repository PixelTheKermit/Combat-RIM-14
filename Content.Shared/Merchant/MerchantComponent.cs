using System.Linq;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server.Merchant
{
    
    [RegisterComponent]
    public sealed class MerchantComponent : Component
    {
        [DataField("Currency")]
        public string Currency = string.Empty;
        [DataField("Products", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
        public List<string> Products = new();
    }
}
