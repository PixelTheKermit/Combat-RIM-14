namespace Content.Server.ControllerDevice;

[RegisterComponent]
public sealed class ControllerStructureComponent : Component
{
    [DataField("range")]
    public float Range = 150f;

    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? Controlling;
}
