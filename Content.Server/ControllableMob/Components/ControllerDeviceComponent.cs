namespace Content.Server.ControllerDevice;

[RegisterComponent]
public sealed class ControllerDeviceComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? Controlling;
}
