namespace Content.Server._CombatRim.Control.Components;

[RegisterComponent]
public sealed class CanControlComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? Controlling;
}
