using Content.Server._CombatRim.SoulTrapping.Components;
using Content.Server.DoAfter;
using Content.Server.Light.Components;
using Content.Server.Mind.Components;
using Content.Server.MobState;
using Content.Server.Popups;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Robust.Server.Containers;
using Robust.Server.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;
using System;
using System.Threading;

namespace Content.Server._CombatRim.SoulTrapping;

public sealed class TrappableSoulSystem : EntitySystem
{
    // Dependencies
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly MobStateSystem _stateSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly ItemSlotsSystem _slotsSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize() // VERY IMPORTANT!!!!!!
    {
        base.Initialize();

        SubscribeLocalEvent<TrappableSoulComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<TrappableSoulComponent, TrappingComplete>(OnTrappingCompleted);
        SubscribeLocalEvent<TrappableSoulComponent, TrappingCancelledEvent>(OnTrappingCancelled);
    }

    private void OnInteractUsing(EntityUid uid, TrappableSoulComponent comp, InteractUsingEvent args)
    {
        if (comp.CancelToken != null || !TryComp<SoulTrapperComponent>(args.Used, out var trapperComponent)
            || args.User == uid)
            return;

        if (!_slotsSystem.TryGetSlot(args.Used, "soul-container", out var itemSlot) || !itemSlot.HasItem)
        {
            _popupSystem.PopupEntity(Loc.GetString("soul-trapping-fail-no-container"), args.User, args.User);
            return;
        }

        var delay = comp.CaptureTime;

        if (_stateSystem.IsAlive(uid) || _stateSystem.IsSoftCrit(uid))
            delay = comp.AliveCaptureTime;

        _popupSystem.PopupEntity(Loc.GetString("soul-trapping-trapping-soul-trapper"), args.User, args.User);
        _popupSystem.PopupEntity(Loc.GetString("soul-trapping-trapping-soul-victim"), uid, uid);

        comp.CancelToken = new CancellationTokenSource();
        _doAfterSystem.DoAfter(new DoAfterEventArgs(uid, delay, comp.CancelToken.Token, args.User, args.Used)
        {
            UserFinishedEvent = new TrappingComplete(args.Used, args.User, uid, itemSlot),
            UserCancelledEvent = new TrappingCancelledEvent(),
            BreakOnDamage = true,
            BreakOnTargetMove = true,
            BreakOnUserMove = true,
            BreakOnStun = true,
            NeedHand = true,
            ExtraCheck = () => DoAfterExtraChecks(args.Used),
        }); ; ; ;
    }

    private bool DoAfterExtraChecks(EntityUid used)
    {
        if (!_slotsSystem.TryGetSlot(used, "soul-container", out var itemSlot) || !itemSlot.HasItem)
            return false;

        return true;
    }

    private void OnTrappingCompleted(EntityUid uid, TrappableSoulComponent component, TrappingComplete args)
    {
        component.CancelToken = null;

        var mind = Comp<MindComponent>(uid).Mind;

        var mind2 = Comp<MindComponent>(args.ItemSlot.Item!.Value).Mind;

        if (mind != null)
            mind.TransferTo(args.ItemSlot.Item);

        if (mind2 != null)
            mind2.TransferTo(uid);

        _popupSystem.PopupEntity(Loc.GetString("soul-trapping-trapping-soul-trapper-success"), args.User, args.User);
        _popupSystem.PopupEntity(Loc.GetString("soul-trapping-trapping-soul-victim-success"), uid, uid);
    }

    private void OnTrappingCancelled(EntityUid uid, TrappableSoulComponent component, TrappingCancelledEvent args)
    {
        component.CancelToken = null;
    }

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

    private sealed class TrappingCancelledEvent : EntityEventArgs { }
}
