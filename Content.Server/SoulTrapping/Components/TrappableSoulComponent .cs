using System.Threading;

namespace Content.Server.SoulTrapping;

[RegisterComponent]
public sealed class TrappableSoulComponent : Component
{
    /// <summary>
    /// The capture time when the entity is alive, this should be long.
    /// </summary>
    [DataField("aliveCaptureTime")]
    public float AliveCaptureTime = 60f;

    /// <summary>
    /// The capture time when the entity is down. 
    /// </summary>
    [DataField("unconsciousCaptureTime")]
    public float CaptureTime = 15f;

    [DataField("insertSound")]
    public string InsertSound = "";

    [DataField("removeSound")]
    public string RemoveSound = "";

    public CancellationTokenSource? CancelToken;
}
