using System.Threading;

namespace Content.Server._CombatRim.ControllableMob.Components;

[RegisterComponent]
public sealed class ControllerDeviceComponent : Component
{
    /// <summary>
    /// Useful to make a sort of "connector" item, and doesn't force me to make another system & component. 
	/// (This should probably be split into it's own component)
    /// </summary>
    [DataField("enabled")]
    public bool Enabled = true;

    [DataField("range")]
    public float Range = 50f;

    [DataField("controlling")]
    public EntityUid? Controlling;

    [DataField("delayMultiplier")]
    public float Multiplier = 1f;
}
