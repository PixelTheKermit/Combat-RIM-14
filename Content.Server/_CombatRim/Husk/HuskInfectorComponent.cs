namespace Content.Server._CombatRim.Husk
{
    [RegisterComponent]
    public sealed class HuskInfectorComponent : Component
    {
        [DataField("infChance")]
        public float InfChance = 0.1f;
    }
}
