using Content.Server.GameTicking;
using Content.Server.Ghost.Components;
using Content.Shared._Afterlight;
using Content.Shared.Administration;
using Content.Shared.GameTicking;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Timing;

namespace Content.Server._Afterlight.Commands;

[AnyCommand]
public sealed class GhostRespawnCommand : IConsoleCommand
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IEntitySystemManager _systemManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public string Command => "ghostrespawn";
    public string Description => "Respawns you, if eligible.";
    public string Help => "ghostrespawn";
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (!_cfg.GetCVar(AfterlightCVarsShared.RespawnEnabled))
        {
            shell.WriteError("Respawning is not enabled.");
            return;
        }

        var gameTicker = _systemManager.GetEntitySystem<GameTicker>();
        var player = (IPlayerSession) shell.Player!;
        if (player.AttachedEntity is not { } playerEnt)
        {
            shell.WriteError("You don't have an attached entity, can't respawn.");
            if (gameTicker.PlayerGameStatuses[player.UserId] == PlayerGameStatus.JoinedGame)
            {
                shell.WriteLine("However you appear to be stuck, so this will bail you out anyway.");
                gameTicker.Respawn(player);
            }
            return;
        }

        if (!_ent.TryGetComponent<GhostComponent>(playerEnt, out var ghost))
        {
            shell.WriteError("You're not a ghost, refusing to respawn you.");
            return;
        }

        var minSinceDeath = (_timing.CurTime - ghost.TimeOfDeath).TotalMinutes;
        var reqTime = _cfg.GetCVar(AfterlightCVarsShared.RespawnTime);

        if (minSinceDeath >= reqTime && gameTicker.PlayerGameStatuses[player.UserId] == PlayerGameStatus.JoinedGame)
        {
            gameTicker.Respawn(player);
        }
        else
        {
            shell.WriteError($"It's only been {minSinceDeath:F1} minutes since you died, you need to wait {reqTime:F1} minutes. You got {reqTime - minSinceDeath:F1} minutes left.");
        }
    }
}
