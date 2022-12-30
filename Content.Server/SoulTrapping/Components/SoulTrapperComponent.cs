namespace Content.Server.SoulTrapping;

[RegisterComponent]
public sealed class SoulTrapperComponent : Component
{
    [DataField("capturingMultiplier")]
    public float Multiplier = 1f;
}
