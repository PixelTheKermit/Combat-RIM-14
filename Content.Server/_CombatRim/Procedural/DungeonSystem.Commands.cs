using System.Threading.Tasks;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Decals;
using Content.Shared.Procedural;
using Content.Shared.Procedural.DungeonGenerators;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server.Procedural;

public sealed partial class DungeonSystem
{
    /// <summary>
    /// Generates a dungeon in space via command.
    /// </summary>

    [AdminCommand(AdminFlags.Fun)]
    private async void GenerateSpaceDungeon(IConsoleShell shell, string argstr, string[] args) // TODO: Convert this into an entity spawner
    {
        if (args.Length < 1)
        {
            shell.WriteError("cmd-dungen-arg-count");
            return;
        }

        if (!_prototype.TryIndex<DungeonConfigPrototype>(args[0], out var dungeon))
        {
            shell.WriteError(Loc.GetString("cmd-dungen-config"));
            return;
        }

        if (shell.Player == null || shell.Player.AttachedEntity == null)
        {
            shell.WriteError(Loc.GetString("cmd-dungen-pos"));
            return;
        }

        int seed;

        if (args.Length >= 2)
        {
            if (!int.TryParse(args[1], out seed))
            {
                shell.WriteError(Loc.GetString("cmd-dungen-seed"));
                return;
            }
        }
        else
        {
            seed = new Random().Next();
        }

        var xform = Comp<TransformComponent>(shell.Player.AttachedEntity.Value);

        var mapGrid = _mapManager.CreateGrid(xform.MapID);
        var position = _transform.GetWorldPosition(xform).Floored();
        shell.WriteLine(Loc.GetString("cmd-dungen-start", ("seed", seed)));
        GenerateDungeon(dungeon, mapGrid.Owner, mapGrid, position, seed);
    }
}
