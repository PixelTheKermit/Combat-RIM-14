namespace Content.Server._CombatRim.EatWhitelist
{
    [RegisterComponent]
    public sealed class EatWhitelistComponent : Component
    {
        [DataField("stomachs", required: true)]

        public List<string> Stomachs = new();
    }
}
