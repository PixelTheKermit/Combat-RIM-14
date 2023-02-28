using System.Threading;
using Content.Server._CombatRim.SoulTrapping.Components;
using Content.Server.DoAfter;
using Content.Server.Mind.Components;
using Content.Server.Popups;
using Content.Shared._CombatRim.SoulTrapping.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Internal.TypeSystem;
using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server._CombatRim.SoulTrapping;

public sealed class SoulTrapperSystem : EntitySystem
{
    // Dependencies
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly ItemSlotsSystem _slotsSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly MobStateSystem _stateSystem = default!;

    public override void Initialize() // VERY IMPORTANT!!!!!!
    {
        base.Initialize();

        SubscribeLocalEvent<SoulTrapperComponent, EntInsertedIntoContainerMessage>(OnInsert);
        SubscribeLocalEvent<SoulTrapperComponent, EntRemovedFromContainerMessage>(OnEject);
        SubscribeLocalEvent<SoulTrapperComponent, AfterInteractEvent>(GetInteraction);
        SubscribeLocalEvent<SoulTrapperComponent, DoAfterEvent>(OnDoAfter);
    }

    private void GetInteraction(EntityUid uid, SoulTrapperComponent comp, AfterInteractEvent args)
    {
        if (comp.CancelToken != null || args.Target == null || args.Target == args.User ||
            !TryComp<TrappableSoulComponent>(args.Target, out var soulComp))
            return;

        if (!_slotsSystem.TryGetSlot(uid, "soul-container", out var itemSlot) || !itemSlot.HasItem)
        {
            _popupSystem.PopupEntity(Loc.GetString("soul-trapping-fail-no-container"), args.User, args.User);
            return;
        }

        var delay = soulComp.CaptureTime;

        if (_stateSystem.IsAlive(uid))
            delay = soulComp.AliveCaptureTime;

        _popupSystem.PopupEntity(Loc.GetString("soul-trapping-trapping-soul-trapper"), args.User, args.User);
        _popupSystem.PopupEntity(Loc.GetString("soul-trapping-trapping-soul-victim"), args.Target.Value, args.Target.Value);

        comp.CancelToken = new CancellationTokenSource();

        var eventArgs = new DoAfterEventArgs(args.User, delay, comp.CancelToken.Token, args.Target, uid)
        {
            BreakOnDamage = true,
            BreakOnTargetMove = true,
            BreakOnUserMove = true,
            BreakOnStun = true,
            NeedHand = true,
            ExtraCheck = () => DoAfterExtraChecks(args.Used),
        };

        _doAfterSystem.DoAfter(eventArgs);
    }

    private bool DoAfterExtraChecks(EntityUid used)
    {
        if (!_slotsSystem.TryGetSlot(used, "soul-container", out var itemSlot) || !itemSlot.HasItem)
            return false;

        return true;
    }

    private void OnInsert(EntityUid uid, SoulTrapperComponent comp, EntInsertedIntoContainerMessage args)
    {
        _appearanceSystem.SetData(uid, SoulTrapperVisuals.Inserted, 1);

        if (Comp<MindComponent>(args.Entity).Mind != null)
        {
            _appearanceSystem.SetData(uid, SoulTrapperVisuals.Inserted, 2);
        }

        Dirty(uid);
    }

    private void OnEject(EntityUid uid, SoulTrapperComponent comp, EntRemovedFromContainerMessage args)
    {
        _appearanceSystem.SetData(uid, SoulTrapperVisuals.Inserted, 0);
        Dirty(uid);
    }

    private void OnDoAfter(EntityUid uid, SoulTrapperComponent component, DoAfterEvent args)
    {
        component.CancelToken = null;

        if (args.Cancelled || args.Args.Target == null || !_slotsSystem.TryGetSlot(uid, "soul-container", out var itemSlot))
            return;

        var mind = Comp<MindComponent>(args.Args.Target.Value).Mind;

        var mind2 = Comp<MindComponent>(itemSlot.Item!.Value).Mind;

        if (mind != null)
            mind.TransferTo(itemSlot.Item);

        if (mind2 != null)
            mind2.TransferTo(args.Args.Target);

        UpdateTrapperVisuals(uid, itemSlot.Item.Value, mind);

        _popupSystem.PopupEntity(Loc.GetString("soul-trapping-trapping-soul-trapper-success"), args.Args.User, args.Args.User);
        _popupSystem.PopupEntity(Loc.GetString("soul-trapping-trapping-soul-victim-success"), uid, uid);
    }

    /// <summary>
    /// Update the visuals for the soul trapper + the bottle in it
    /// </summary>
    private void UpdateTrapperVisuals(EntityUid uid, EntityUid contain, Mind.Mind? mind)
    {
        _appearanceSystem.SetData(uid, SoulTrapperVisuals.Inserted, 1);
        _appearanceSystem.SetData(contain, SoulTrapperVisuals.Inserted, 0);

        if (mind != null)
        {
            _appearanceSystem.SetData(uid, SoulTrapperVisuals.Inserted, 2);
            _appearanceSystem.SetData(contain, SoulTrapperVisuals.Inserted, 1);
        }

        Dirty(uid);
        Dirty(contain);
    }

    /// <summary>
    /// Called when soul trapping doesn't fail
    /// </summary>

    private sealed class TrappingComplete : EntityEventArgs
    {
        public EntityUid Used { get; }
        public EntityUid User { get; }
        public EntityUid Target { get; }
        public ItemSlot ItemSlot { get; }

        public TrappingComplete(EntityUid used, EntityUid user, EntityUid target, ItemSlot itemSlot)
        {
            Used = used;
            User = user;
            Target = target;
            ItemSlot = itemSlot;
        }
    }


    /// <summary>
    /// Called when soul trapping fails
    /// </summary>
    private sealed class TrappingCancelledEvent : EntityEventArgs { }
}
