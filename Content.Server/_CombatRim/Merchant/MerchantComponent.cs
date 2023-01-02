using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server._CombatRim.Merchant;
    
[RegisterComponent]
public sealed class MerchantComponent : Component
{
    [DataField("Currency")]
    public string Currency = string.Empty;
    [DataField("Products", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
    public List<string> Products = new();
    [DataField("Index")]
    public int Index = 0;
    [DataField("ProductsCount")]
    public int Count = 20;
}
