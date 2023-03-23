

namespace Content.Server._CombatRim.Husk
{
    [RegisterComponent]
    public sealed class HuskHostComponent : Component
    {
        [DataField("stage")]
        public int Stage = 0;

        public TimeSpan? LastStage;
    }
}
