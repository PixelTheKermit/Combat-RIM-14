
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Ghost.Components;
using Content.Server.Mind;
using Content.Shared.Mobs.Systems;

namespace Content.Server._CombatRim.Rules
{
    public sealed class EscapeRuleSystem : GameRuleSystem
    {
        public override string Prototype => "CREscape";

        [Dependency] private readonly MindTrackerSystem _mindTracker = default!;
        [Dependency] private readonly GameTicker _gameTicker = default!;
        [Dependency] private readonly MobStateSystem _mobStateSys = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndText);
        }

        private void OnRoundEndText(RoundEndTextAppendEvent args)
        {
            List<(string, string)> escapees = new();

            foreach (var mind in _mindTracker.AllMinds)
            {
                var uid = mind.CurrentEntity;

                if (uid == null || !TryComp<TransformComponent>(uid, out var xform) || HasComp<GhostComponent>(uid))
                    continue;

                if (!_mobStateSys.IsDead(uid.Value) && xform.MapID != _gameTicker.DefaultMap) // This means you've escaped! good job!
                {
                    if (mind.TryGetSession(out var session) && mind.CharacterName != null) // Gotta have a session though!
                    {
                        escapees.Add((mind.CharacterName, session.Name));
                    }
                }
            }

            if (escapees.Count == 0) // No one escaped
            {
                args.AddLine(Loc.GetString("cr-end-round-no-escapees"));
                return;
            }

            if (escapees.Count == 1)
                args.AddLine(Loc.GetString("cr-end-round-lone-escapee"));
            else
                args.AddLine(Loc.GetString("cr-end-round-escapees-count", ("count", escapees.Count)));

            foreach (var (name, user) in escapees)
            {
                args.AddLine(Loc.GetString("cr-end-round-escapee", ("name", name), ("user", user))); // TODO: Display how many spacebucks you made it out with
            }
            args.AddLine(Loc.GetString("cr-end-round-postscript"));
        }

        public override void Started() { }
        public override void Ended() { }
    }
}
