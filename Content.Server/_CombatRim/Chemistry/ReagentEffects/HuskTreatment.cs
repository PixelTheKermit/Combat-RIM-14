
using System.Text.Json.Serialization;
using Content.Server._CombatRim.Husk;
using Content.Shared.Chemistry.Reagent;
using JetBrains.Annotations;

namespace Content.Server._CombatRim.Chemistry.ReagentEffects
{
    [UsedImplicitly]
    public sealed class HuskTreatment : ReagentEffect
    {
        [JsonPropertyName("treatmentValue")]
        [DataField("treatmentValue", required: true)]
        public float TreatValue = default!;

        public override void Effect(ReagentEffectArgs args)
        {
            if (args.EntityManager.TryGetComponent<HuskHostComponent>(args.SolutionEntity, out var husk) && husk.LastStage != null)
            {
                var newStage = husk.LastStage + TimeSpan.FromSeconds(TreatValue);
                husk.LastStage = newStage;
            }
        }
    }
}
