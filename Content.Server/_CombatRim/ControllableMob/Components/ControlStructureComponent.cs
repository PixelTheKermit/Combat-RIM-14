namespace Content.Server._CombatRim.Control.Components;

[RegisterComponent]
public sealed class ControlStructureComponent : Component
{
    [DataField("range")]
    public float Range = 150f;

    [DataField("controlling")]
    public EntityUid? Controlling;
}
