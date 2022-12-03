
using Content.Server.Mind.Components;
using Content.Server.Players;
using Content.Shared.Interaction;
using Content.Shared.Verbs;
using Robust.Server.Player;

namespace Content.Server.ControllableMob;

public sealed class ControllableMobSystem : EntitySystem
{
    // Dependencies
    [Dependency] private readonly EntityManager _entityManager = default!;

    public override void Initialize() // VERY IMPORTANT!!!!!!
    {
        base.Initialize();

        SubscribeLocalEvent<ControllableMobComponent, InteractHandEvent>(GetInteraction);
        SubscribeLocalEvent<ControllableMobComponent, GetVerbsEvent<ActivationVerb>>(AddActVerb);
    }

    private void GetInteraction(EntityUid uid, ControllableMobComponent comp, InteractHandEvent args)
    {
        if (comp.CurrentEntityOwning == null && TryComp<ControllerComponent>(args.User, out var controlComp))
        {
            controlComp.Controlling = uid;

            comp.CurrentEntityOwning = args.User;

            var userId = _entityManager.GetComponent<MindComponent>(args.User).Mind!.UserId;

            var session = IoCManager.Resolve<IPlayerManager>().GetSessionByUserId(userId!.Value);

            var playerCData = session.ContentData();

            var mind = playerCData!.Mind;

            if (mind == null)
            {
                mind = new Mind.Mind(session.UserId)
                {
                    CharacterName = _entityManager.GetComponent<MetaDataComponent>(uid).EntityName
                };
                mind.ChangeOwningPlayer(session.UserId);
            }
            mind.TransferTo(uid);
        }
    }

    private void StopControl(EntityUid uid, ControllableMobComponent comp)
    {
        if (comp.CurrentEntityOwning != null && TryComp<ControllerComponent>(comp.CurrentEntityOwning, out var controlComp))
        {
            controlComp.Controlling = null;

            var userId = _entityManager.GetComponent<MindComponent>(uid).Mind!.UserId;

            var session = IoCManager.Resolve<IPlayerManager>().GetSessionByUserId(userId!.Value);

            var playerCData = session.ContentData();

            var mind = playerCData!.Mind;

            if (mind == null)
            {
                mind = new Mind.Mind(session.UserId)
                {
                    CharacterName = _entityManager.GetComponent<MetaDataComponent>(comp.CurrentEntityOwning.Value).EntityName
                };
                mind.ChangeOwningPlayer(session.UserId);
            }

            mind.TransferTo(comp.CurrentEntityOwning.Value);

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
