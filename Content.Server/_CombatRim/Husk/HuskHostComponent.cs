

namespace Content.Server._CombatRim.Husk
{
    [RegisterComponent]
    public sealed class HuskHostComponent : Component
    {
        [DataField("stage")]
        public int Stage = 0;

        /// <summary>
        /// Infection time, in minutes.
        /// </summary>
        [DataField("infectionTime")]
        public float InfectionTime = 1f;

        /// <summary>
        /// The cure threshold, in seconds.
        /// </summary>
        public float CureThreshold = 5f;
        public TimeSpan? LastStage;
    }
}
