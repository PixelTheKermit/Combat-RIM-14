namespace Content.Server._CombatRim.DeathMessage
{
    [RegisterComponent]
    public sealed class DeathMessageComponent : Component
    {
        [DataField("string")]
        public string Message = "universal-death-message";

        public bool HadSuicided = false;
    }
}
