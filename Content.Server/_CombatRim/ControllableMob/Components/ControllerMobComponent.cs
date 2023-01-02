namespace Content.Server._CombatRim.ControllableMob.Components;

[RegisterComponent]
public sealed class ControllerMobComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? Controlling;
}
