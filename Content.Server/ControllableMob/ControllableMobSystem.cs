
using Content.Server.ControllerDevice;
using Content.Server.Database;
using Content.Server.Mind.Components;
using Content.Server.MobState;
using Content.Server.Players;
using Content.Shared.Interaction;
using Content.Shared.MobState;
using Content.Shared.MobState.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Maths;

namespace Content.Server.ControllableMob;

public sealed class ControllableMobSystem : EntitySystem
{
    // Dependencies
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize() // VERY IMPORTANT!!!!!!
    {
        base.Initialize();

        SubscribeLocalEvent<ControllableMobComponent, InteractUsingEvent>(GetInteraction);
        SubscribeLocalEvent<ControllableMobComponent, GetVerbsEvent<ActivationVerb>>(AddActVerb);
        SubscribeLocalEvent<ControllableMobComponent, MobStateChangedEvent>(MobStateChanged);
    }

    public override void Update(float frameTime)
  {
        base.Update(frameTime);

        foreach (var (contMob, xform) in EntityQuery<ControllableMobComponent, TransformComponent>())
        {
            
            if (contMob.CurrentEntityOwning != null)
            {
                var calcDist = (xform.WorldPosition - Comp<TransformComponent>(contMob.CurrentEntityOwning.Value).WorldPosition).Length;

                if (calcDist > contMob.Range)
                {
                    ChangeControl(contMob.Owner, contMob.CurrentEntityOwning.Value);
                    Comp<ControllerMobComponent>(contMob.CurrentEntityOwning.Value).Controlling = null;
                    _popupSystem.PopupEntity(Loc.GetString("device-control-out-of-range"), contMob.CurrentEntityOwning.Value, Filter.Entities(contMob.Owner));
                    contMob.CurrentEntityOwning = null;
                }
            }
        }
    }

    public void ChangeControl(EntityUid former, EntityUid latter)
    {
        if (!_entityManager.TryGetComponent<MindComponent>(former, out var mindComp))
            return;

        var userId = mindComp.Mind!.UserId;

        var session = IoCManager.Resolve<IPlayerManager>().GetSessionByUserId(userId!.Value);

        var playerCData = session.ContentData();

        var mind = playerCData!.Mind;

        if (mind == null)
        {
            mind = new Mind.Mind(session.UserId)
            {
                CharacterName = _entityManager.GetComponent<MetaDataComponent>(latter).EntityName
            };
            mind.ChangeOwningPlayer(session.UserId);
        }
        mind.TransferTo(latter);
    }

    private void MobStateChanged(EntityUid uid, ControllableMobComponent comp, MobStateChangedEvent args)
    {
        if (comp.CurrentEntityOwning != null && args.CurrentMobState == DamageState.Dead &&
            TryComp<ControllerMobComponent>(comp.CurrentEntityOwning, out var controlComp))
        {
            controlComp.Controlling = null;

            ChangeControl(uid, comp.CurrentEntityOwning.Value);

            comp.CurrentEntityOwning = null;
        }
    }

    private void GetInteraction(EntityUid uid, ControllableMobComponent comp, InteractUsingEvent args)
    {
        if (comp.CurrentEntityOwning == null && TryComp<ControllerMobComponent>(args.User, out var controlComp) &&
            !(TryComp<MobStateComponent>(uid, out var damageState) && _mobStateSystem.IsDead(uid, damageState))
            && TryComp<ControllerDeviceComponent>(args.Used, out var controlDeviceComp))
        {
            controlDeviceComp.Controlling = uid;
            _popupSystem.PopupEntity(Loc.GetString("device-control-paired"), uid, Filter.Entities(args.User));
        }
    }

    private void StopControl(EntityUid uid, ControllableMobComponent comp)
    {
        if (comp.CurrentEntityOwning != null && TryComp<ControllerMobComponent>(comp.CurrentEntityOwning, out var controlComp))
        {
            controlComp.Controlling = null;

            ChangeControl(uid, comp.CurrentEntityOwning.Value);

            comp.CurrentEntityOwning = null;
        }
    }

    private void AddActVerb(EntityUid uid, ControllableMobComponent comp, GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess ||
            args.User != uid || !_entityManager.EntityExists(uid))
            return;

        ActivationVerb verb = new()
        {
            Act = () =>
            {
                StopControl(uid, comp);
            },
            Text = "Stop controlling",
            Priority = 1
        };
        args.Verbs.Add(verb);
    }
}
