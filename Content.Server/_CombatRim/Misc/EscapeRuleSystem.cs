
using System.Linq;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Ghost.Components;
using Content.Server.Mind;
using Content.Server.Stack;
using Content.Server.Store.Components;
using Content.Shared.Containers;
using Content.Shared.Mobs.Systems;
using Content.Shared.Stacks;
using Robust.Server.Containers;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server._CombatRim.Rules
{
    public sealed class EscapeRuleSystem : GameRuleSystem<EscapeRuleComponent>
    {
        [Dependency] private readonly MindTrackerSystem _mindTracker = default!;
        [Dependency] private readonly GameTicker _gameTicker = default!;
        [Dependency] private readonly MobStateSystem _mobStateSys = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly ContainerSystem _containerSystem = default!;
        [Dependency] private readonly StackSystem _stackSystem = default!;
        [Dependency] private readonly EntityManager _entityManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
        }

        private void OnRoundEndText(RoundEndTextAppendEvent args)
        {
            var query = EntityQueryEnumerator<EscapeRuleComponent, GameRuleComponent>();

            while (query.MoveNext(out var gameRuleUid, out var escRule, out var gameRule))
            {
                if (!GameTicker.IsGameRuleAdded(gameRuleUid, gameRule))
                    continue;

                List<(string, string, int)> escapees = new();

                foreach (var mind in _mindTracker.AllMinds)
                {
                    var uid = mind.CurrentEntity;

                    if (uid == null || !TryComp<TransformComponent>(uid, out var xform) || HasComp<GhostComponent>(uid))
                        continue;

                    if (!_mobStateSys.IsDead(uid.Value) && xform.MapID != _gameTicker.DefaultMap && // This means you've escaped! good job!
                        mind.TryGetSession(out var session) && mind.CharacterName != null) // Gotta have a session though!
                    {
                        List<EntityUid> storedUids = new();

                        var totalCash = 0;

                        RecursiveAdd(uid.Value, storedUids);

                        foreach (var ent in storedUids)
                        {
                            if (TryComp<CurrencyComponent>(ent, out var curComp) && curComp.Price.TryGetValue(_cfg.GetCVar<string>(CombatRimCVars.MainCurrency), out var cash))
                            {
                                if (TryComp<StackComponent>(ent, out var stackComp))
                                    totalCash += (int) cash*stackComp.Count;
                                else
                                    totalCash += (int) cash;
                            }
                        }

                        escapees.Add((mind.CharacterName, session.Name, totalCash));
                    }
                }

                if (escapees.Count == 0) // No one escaped
                {
                    args.AddLine(Loc.GetString("cr-end-round-no-escapees"));
                    return;
                }

                if (escapees.Count == 1) // One escapee
                    args.AddLine(Loc.GetString("cr-end-round-lone-escapee"));
                else // Multiple escapees
                    args.AddLine(Loc.GetString("cr-end-round-escapees-count", ("count", escapees.Count)));

                foreach (var (name, user, cash) in escapees)
                {
                    args.AddLine(Loc.GetString("cr-end-round-escapee", ("name", name), ("user", user), ("cash", cash)));
                }
                args.AddLine(Loc.GetString("cr-end-round-postscript"));
            }
        }

        private void RecursiveAdd(EntityUid uid, List<EntityUid> toAdd)
        {
            if (!HasComp<ContainerManagerComponent>(uid))
                return;

            foreach (var container in _containerSystem.GetAllContainers(uid))
            {
                foreach (var ent in container.ContainedEntities)
                {
                    toAdd.Add(ent);
                    RecursiveAdd(ent, toAdd);
                }
            }
        }
    }
}
