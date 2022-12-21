namespace Content.Server.ControllerDevice;

[RegisterComponent]
public sealed class ControllerStructureComponent : Component
{
    [DataField("range")]
    public float Range = 150f;

    [DataField("controlling")]
    public EntityUid? Controlling;
}
