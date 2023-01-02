namespace Content.Server._CombatRim.ControllableMob.Components;

[RegisterComponent]
public sealed class ControllableMobComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public float Range = 50f;

    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? CurrentEntityOwning;

    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? CurrentDeviceOwning;
}
