using Content.Server._00OuterRim.Worldgen.Prototypes;
using Content.Server._00OuterRim.Worldgen.Systems;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._00OuterRim.Worldgen.Commands;

[AnyCommand]
public sealed class SpawnDebrisCommand : IConsoleCommand
{
    public string Command => "spawndebris";

    public string Description => Loc.GetString("spawn-debris-command-description");

    public string Help => Loc.GetString("spawn-debris-command-help");

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var pos = shell.Player?.AttachedEntityTransform?.MapPosition;
        if (pos is null)
        {
            shell.WriteError(Loc.GetString("shell-only-players-can-run-this-command"));
            return;
        }

        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        if (!prototypeManager.TryIndex<DebrisPrototype>(args[0], out var proto))
        {
            shell.WriteError(Loc.GetString("shell-argument-must-be-prototype", ("index", 1), ("prototypeName", "debris prototype")));
            return;
        }

        EntitySystem.Get<DebrisGenerationSystem>().GenerateDebris(proto, pos.Value);
    }
}
