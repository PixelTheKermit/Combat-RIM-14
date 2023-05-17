
using System.ComponentModel.DataAnnotations;
using Content.Shared.FixedPoint;
using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server._CombatRim.Economy
{
    [Serializable, Prototype("randListings")]
    public sealed class RandListingsPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField("listings", required: true)]
        public List<(string ID, float Chance)> Listings = new();
    }
}
