namespace Content.Server.ControllerDevice;

[RegisterComponent]
public sealed class ControllerDeviceComponent : Component
{
    [DataField("range")]
    public float Range = 50f;

    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? Controlling;
}
