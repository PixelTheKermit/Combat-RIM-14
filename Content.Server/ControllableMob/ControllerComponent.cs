namespace Content.Server.ControllableMob;

[RegisterComponent]
public sealed class ControllerComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? Controlling;
}
