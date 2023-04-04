using System.Threading;

namespace Content.Server._CombatRim.Control.Components;

[RegisterComponent]
public sealed class ControllableComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? CurrentEntityOwning;

    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? CurrentDeviceOwning;

    [DataField("delay")]
    public float Delay = 15;

    public float Range = 0f;
}
