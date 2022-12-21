namespace Content.Server.ControllerDevice;

[RegisterComponent]
public sealed class ControllerDeviceComponent : Component
{
    /// <summary>
    /// Useful to make a sort of "connector" item, and doesn't force me to make another system & component.
    /// </summary>
    [DataField("enabled")]
    public bool Enabled = true;

    [DataField("range")]
    public float Range = 50f;

    [DataField("controlling")]
    public EntityUid? Controlling;
}
