
using System.Text.Json.Serialization;
using Content.Server._CombatRim.Husk;
using Content.Server.GameTicking;
using Content.Shared.Chemistry.Reagent;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._CombatRim.Chemistry.ReagentEffects
{
    [UsedImplicitly]
    public sealed class HuskTreatment : ReagentEffect
    {
        [JsonPropertyName("treatmentValue")]
        [DataField("treatmentValue", required: true)]
        public float TreatValue = default!;

        protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        {
            return Loc.GetString("cr-reagent-effect-guidebook-husk-treatment");
        }

        public override void Effect(ReagentEffectArgs args)
        {
            if (args.EntityManager.TryGetComponent<HuskHostComponent>(args.SolutionEntity, out var host) && host.LastStage != null)
            {
                var curtime = IoCManager.Resolve<IGameTiming>().CurTime;
                if (host.LastStage + TimeSpan.FromMinutes(host.InfectionTime) > curtime)
                    host.LastStage = curtime;
                var newStage = host.LastStage + TimeSpan.FromSeconds(TreatValue);
                host.LastStage = newStage;
            }
        }
    }
}
