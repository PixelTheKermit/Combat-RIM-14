using Content.Client.Stylesheets;
using Content.Client.UserInterface.Systems.Ghost.Controls;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Timing;
using Robust.Shared.Configuration;
using Content.Shared.CCVar;

namespace Content.Client.UserInterface.Systems.Ghost.Widgets;

[GenerateTypedNameReferences]
public sealed partial class GhostGui : UIWidget
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    public GhostTargetWindow TargetWindow { get; }

    public event Action? RequestWarpsPressed;
    public event Action? ReturnToBodyPressed;
    public event Action? GhostRolesPressed;
    public event Action? RespawnPressed;

    public GhostGui()
    {
        RobustXamlLoader.Load(this);

        TargetWindow = new GhostTargetWindow();

        MouseFilter = MouseFilterMode.Ignore;

        GhostWarpButton.OnPressed += _ => RequestWarpsPressed?.Invoke();
        ReturnToBodyButton.OnPressed += _ => ReturnToBodyPressed?.Invoke();
        GhostRolesButton.OnPressed += _ => GhostRolesPressed?.Invoke();
        RespawnButton.OnPressed += _ => RespawnPressed?.Invoke();
    }

    public void Hide()
    {
        TargetWindow.Close();
        Visible = false;
    }


    public void UpdateRespawn(TimeSpan? todd)
    {
        if (todd != null)
        {
            var time = (_gameTiming.CurTime - todd);
            var respawnTime = _configurationManager.GetCVar(CCVars.RespawnTime);
            RespawnButton.Disabled = respawnTime > time.Value.TotalSeconds;
            RespawnButton.Text = RespawnButton.Disabled
                ? Loc.GetString("ghost-gui-respawn-button-denied", ("time", (int) (respawnTime - time.Value.TotalSeconds)))
                : Loc.GetString("ghost-gui-respawn-button-allowed");
        }
    }

    public void Update(int? roles, bool? canReturnToBody) // Don't ask me why I named this variable Todd
    {
        ReturnToBodyButton.Disabled = !canReturnToBody ?? true;

        if (roles != null)
        {
            GhostRolesButton.Text = Loc.GetString("ghost-gui-ghost-roles-button", ("count", roles));
            if (roles > 0)
            {
                GhostRolesButton.StyleClasses.Add(StyleBase.ButtonCaution);
            }
            else
            {
                GhostRolesButton.StyleClasses.Remove(StyleBase.ButtonCaution);
            }
        }

        TargetWindow.Populate();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            TargetWindow.Dispose();
        }
    }
}