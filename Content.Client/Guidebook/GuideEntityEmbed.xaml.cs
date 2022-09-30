using System.Diagnostics.CodeAnalysis;
using Content.Client.Examine;
using Content.Client.Guidebook.Richtext;
using Content.Client.Verbs;
using Content.Shared.Input;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Map;

namespace Content.Client.Guidebook;


[GenerateTypedNameReferences]
public sealed partial class GuideEntityEmbed : BoxContainer, ITag
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IEntitySystemManager _systemManager = default!;

    private readonly VerbSystem _verbSystem;
    private readonly ExamineSystem _examineSystem;
    private readonly GuidebookSystem _guidebookSystem;

    public bool Interactive;

    public ISpriteComponent? Sprite
    {
        get => View.Sprite;
        set => View.Sprite = value;
    }

    public Vector2 Scale
    {
        get => View.Scale;
        set => View.Scale = value;
    }

    public GuideEntityEmbed()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
        _verbSystem = _systemManager.GetEntitySystem<VerbSystem>();
        _examineSystem = _systemManager.GetEntitySystem<ExamineSystem>();
        _guidebookSystem = _systemManager.GetEntitySystem<GuidebookSystem>();
        MouseFilter = MouseFilterMode.Stop;
    }

    public GuideEntityEmbed(string proto, bool caption, bool interactive) : this()
    {
        Interactive = interactive;

        var ent = _entityManager.SpawnEntity(proto, MapCoordinates.Nullspace);
        Sprite = _entityManager.GetComponent<SpriteComponent>(ent);

        if (caption)
            Caption.Text = _entityManager.GetComponent<MetaDataComponent>(ent).EntityName;
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);
        // get an entity associated with this element
        var entity = Sprite?.Owner;

        // Deleted() automatically checks for null & existence.
        if (_entityManager.Deleted(entity))
            return;

        // do examination?
        if (args.Function == ContentKeyFunctions.ExamineEntity)
        {
            _examineSystem.DoExamine(entity.Value);
            args.Handle();
            return;
        }

        if (!Interactive)
            return;

        // open verb menu?
        if (args.Function == ContentKeyFunctions.OpenActionsMenu)
        {
            _verbSystem.VerbMenu.OpenVerbMenu(entity.Value);
            args.Handle();
            return;
        }

        // from here out we're faking interactions! sue me. --moony

        if (args.Function == ContentKeyFunctions.ActivateItemInWorld)
        {
            _guidebookSystem.FakeClientActivateInWorld(entity.Value);
            _verbSystem.CloseAllMenus();
            args.Handle();
            return;
        }

        if (args.Function == ContentKeyFunctions.AltActivateItemInWorld)
        {
            _guidebookSystem.FakeClientAltActivateInWorld(entity.Value);
            _verbSystem.CloseAllMenus();
            args.Handle();
            return;
        }

        if (args.Function == ContentKeyFunctions.AltActivateItemInWorld)
        {
            _guidebookSystem.FakeClientUse(entity.Value);
            _verbSystem.CloseAllMenus();
            args.Handle();
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (Sprite is not null)
            _entityManager.DeleteEntity(Sprite.Owner);
    }

    public bool TryParseTag(List<string> args, Dictionary<string, string> param, [NotNullWhen(true)] out Control? control, out bool instant)
    {
        instant = true;
        if (args.Count is < 1 or > 3)
        {
            Logger.Error($"GuideEntityEmbed expected at least one argument and at most three, got {args.Count}");
            control = null;
            return false;
        }

        var proto = args[0];
        var caption = args.Contains("Caption");
        var interactive = args.Contains("Interactive");

        Interactive = interactive;

        var ent = _entityManager.SpawnEntity(proto, MapCoordinates.Nullspace);
        Sprite = _entityManager.GetComponent<SpriteComponent>(ent);

        if (caption)
            Caption.Text = _entityManager.GetComponent<MetaDataComponent>(ent).EntityName;

        if (param.TryGetValue("Scale", out var scaleStr) && float.TryParse(scaleStr, out var scale))
        {
            Scale = new Vector2(scale, scale);
        }

        control = this;
        return true;
    }
}
