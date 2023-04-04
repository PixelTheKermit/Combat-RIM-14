using System.Threading;

namespace Content.Server._CombatRim.Control.Components;

[RegisterComponent]
public sealed class RemotePairerComponent : Component
{
    [DataField("storedEntity")]
    public EntityUid? Entity;

    [DataField("devMode")]
    public bool DevMode = false;

    [DataField("delayMultiplier")]
    public float Multiplier = 1f;

    public CancellationTokenSource? CancelToken;
}
