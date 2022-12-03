
using Content.Server.Mind.Components;
using Content.Server.Players;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Verbs;
using Robust.Server.Player;

namespace Content.Server.ControllableMob;

public sealed class ControllerSystem : EntitySystem
{
    // Dependencies
    [Dependency] private readonly EntityManager _entityManager = default!;

    public override void Initialize() // VERY IMPORTANT!!!!!!
    {
        base.Initialize();

        SubscribeLocalEvent<ControllerComponent, DamageChangedEvent>(WasHurt);
    }

    private void WasHurt(EntityUid uid, ControllerComponent comp, DamageChangedEvent args)
    {
        if (comp.Controlling != null && _entityManager.EntityExists(comp.Controlling)
            && TryComp<ControllableMobComponent>(comp.Controlling, out var controllableComp))
        {
            var userId = _entityManager.GetComponent<MindComponent>(comp.Controlling.Value).Mind!.UserId;

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

            controllableComp = null;
        }
    }
}
