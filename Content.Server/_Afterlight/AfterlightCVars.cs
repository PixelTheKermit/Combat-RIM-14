using Robust.Shared.Configuration;

namespace Content.Server._Afterlight;

[CVarDefs]
public sealed class AfterlightCVars
{
    /// <summary>
    /// Whether or not ship spawning is enabled.
    /// </summary>
    public static readonly CVarDef<bool> ShipSpawningEnabled =
        CVarDef.Create("afterlight.shipspawning.enabled", true, CVar.SERVERONLY);

    /// <summary>
    /// Whether or not story management is enabled.
    /// </summary>
    public static readonly CVarDef<bool> StoryEnabled =
        CVarDef.Create("afterlight.story_enabled", true, CVar.SERVERONLY);

    /// <summary>
    /// The path to save the game's story data to.
    /// </summary>
    public static readonly CVarDef<string> StoryMapPath =
        CVarDef.Create("afterlight.story_map_path", "/storydata.yml", CVar.SERVERONLY);

    /// <summary>
    /// The path to save backups of the story data to.
    /// </summary>
    public static readonly CVarDef<string> StoryBackupMapPath =
        CVarDef.Create("afterlight.story_map_backup_path", "/storyBack/storydata.yml", CVar.SERVERONLY);
}
