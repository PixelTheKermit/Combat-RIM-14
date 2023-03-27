using System.Threading;

namespace Content.Server._CombatRim.Control.Components;

[RegisterComponent]
public sealed class ControlHandComponent : Component
{
    [DataField("range")]
    public float Range = 50f;
}
