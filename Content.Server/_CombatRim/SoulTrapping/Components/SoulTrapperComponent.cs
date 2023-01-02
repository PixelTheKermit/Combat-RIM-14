namespace Content.Server._CombatRim.SoulTrapping.Components;

[RegisterComponent]
public sealed class SoulTrapperComponent : Component
{
    [DataField("capturingMultiplier")]
    public float Multiplier = 1f;
}
