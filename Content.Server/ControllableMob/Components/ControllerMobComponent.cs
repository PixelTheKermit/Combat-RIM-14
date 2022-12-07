namespace Content.Server.ControllableMob;

[RegisterComponent]
public sealed class ControllerMobComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? Controlling;
}
