using System.Linq;
using Content.Shared.Merchant;
using Content.Shared.OuterRim.Generator;
using Content.Shared.VendingMachines;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Merchant;
    
[RegisterComponent]
public sealed class MerchantComponent : Component
{
    [DataField("Currency")]
    public string Currency = string.Empty;
    [DataField("Products", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
    public List<string> Products = new();
    [DataField("Index")]
    public int Index = 0;
}
