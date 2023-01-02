namespace Content.Server._CombatRim.ControllableMob.Components;

[RegisterComponent]
public sealed class ControllerStructureComponent : Component
{
    [DataField("range")]
    public float Range = 150f;

    [DataField("controlling")]
    public EntityUid? Controlling;
}
