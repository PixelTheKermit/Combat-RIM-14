
using System.ComponentModel.DataAnnotations;
using Content.Shared.FixedPoint;
using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server._CombatRim.Economy
{
    [Serializable, Prototype("RandListing")]
    public sealed class RandListingPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField("name", required: true)]
        public string Name = string.Empty;

        [DataField("listingID", required: true)]
        public string ListingID = string.Empty;

        [DataField("listingChance")]
        public float ListingChance = 1f;
    }
}
