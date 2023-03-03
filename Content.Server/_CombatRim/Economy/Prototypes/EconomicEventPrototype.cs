
using Robust.Shared.Prototypes;

namespace Content.Server._CombatRim.Economy
{
    [Serializable, Prototype("economicEvent")]
    public sealed class EconomicEventPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField("text")]
        public string Text = "economic crash of OR";

        [DataField("multiplier")]
        public float Multiplier = 1f;

        [DataField("addedOn")]
        public int AddedOn = 0;
    }
}
