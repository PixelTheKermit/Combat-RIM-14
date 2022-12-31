using Robust.Shared.Configuration;

namespace Content.Shared._Afterlight;

[CVarDefs]
public sealed class AfterlightCVarsShared
{
    /// <summary>
    /// Whether or not respawning is enabled.
    /// </summary>
    public static readonly CVarDef<bool> RespawnEnabled =
        CVarDef.Create("afterlight.respawn.enabled", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Respawn time, how long the player has to wait in minutes after death.
    /// </summary>
    public static readonly CVarDef<float> RespawnTime =
        CVarDef.Create("afterlight.respawn.time", 3.0f, CVar.SERVER | CVar.REPLICATED);
}
