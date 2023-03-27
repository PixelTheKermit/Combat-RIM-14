
using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.GameTicking;
using Content.Server.Ghost;
using Content.Server.Mind;
using Content.Server.Mind.Components;
using Content.Server.NPC;
using Content.Server.NPC.Components;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Server.Nutrition.Components;
using Content.Server.Popups;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Movement.Components;
using Content.Shared.NPC;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Robust.Shared.Timing;

namespace Content.Server._CombatRim.Husk
{
    /// <summary>
    /// WARNING: BY ACCESSING THIS, YOU ARE EXPOSING YOURSELF TO POSSIBLE SHITCODE.
    /// YOU HAVE BEEN WARNED.
    /// </summary>
    public sealed class HuskInfectionSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly DamageableSystem _damageSystem = default!;
        [Dependency] private readonly GameTicker _gameTicker = default!;
        [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
        [Dependency] private readonly NPCSystem _npcSystem = default!;
        [Dependency] private readonly FactionSystem _factionSystem = default!;
        [Dependency] private readonly EntityManager _entityManager = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<HuskHostComponent, ComponentInit>(OnInitialInfected);
            SubscribeLocalEvent<HuskHostComponent, SpeakAttemptEvent>(OnHostSpeak);
            SubscribeLocalEvent<HuskHostComponent, MobStateChangedEvent>(OnStateChanged);

            SubscribeLocalEvent<HuskifyComponent, ComponentInit>(OnHuskifyComp);
        }

        private void OnInitialInfected(EntityUid uid, HuskHostComponent comp, ComponentInit args)
        {
            comp.LastStage = _gameTiming.CurTime;
        }

        private void OnHuskifyComp(EntityUid uid, HuskifyComponent comp, ComponentInit args)
        {
            HuskifyEntity(uid);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var hosts = EntityQueryEnumerator<HuskHostComponent>();

            DamageSpecifier damage = new();
            damage.DamageDict.Add("Piercing", 3f); // ! Damage should be high.

            while (hosts.MoveNext(out var uid, out var host))
            {
                if (HasComp<HuskifiedComponent>(uid))
                    continue;

                if (host.LastStage + TimeSpan.FromMinutes(host.InfectionTime) < _gameTiming.CurTime)
                {
                    if (host.Stage < 4)
                    {
                        host.LastStage = _gameTiming.CurTime;
                        host.Stage += 1;
                    }
                }
                else if (host.LastStage - TimeSpan.FromSeconds(host.CureThreshold) > _gameTiming.CurTime)
                {
                    host.LastStage = _gameTiming.CurTime + TimeSpan.FromMinutes(host.InfectionTime) - TimeSpan.FromSeconds(host.CureThreshold);
                    if (host.Stage > -1)
                        host.Stage -= 1;
                }

                if (host.Stage >= 4)
                    _damageSystem.TryChangeDamage(uid, damage*frameTime, true, true);
            }

            damage = new(); // reuse the same specifier because yeah.
            damage.DamageDict.Add("Blunt", -.5f); // ! Healing should be slow.

            var husks = EntityQueryEnumerator<HuskifiedComponent>(); // ? This is made seperate because a person may have "Huskified" but somehow not "HuskHost"

            while (husks.MoveNext(out var uid, out var _))
            {
                _damageSystem.TryChangeDamage(uid, damage*frameTime, true, true);
            }
        }

        private void OnHostSpeak(EntityUid uid, HuskHostComponent comp, SpeakAttemptEvent args)
        {
            if (comp.Stage >= 2)
            {
                _popupSystem.PopupEntity(Loc.GetString("husk-attempt-speak"), uid, uid, PopupType.MediumCaution);
                args.Cancel(); // You don't get to speak, 1984.
            }
        }

        private void OnStateChanged(EntityUid uid, HuskHostComponent comp, MobStateChangedEvent args)
        {
            if (args.NewMobState == MobState.Dead && !HasComp<HuskifiedComponent>(uid))
                HuskifyEntity(uid);
        }

        public void HuskifyEntity(EntityUid uid)
        {
            if (HasComp<HuskifiedComponent>(uid))
                return;

            // You've conformed to the husk. You are no longer the host of your own body.
            AddComp<HuskifiedComponent>(uid);

            // The parasite probably doesn't care if you can't breathe or not, it just needs a vessel.
            RemComp<BarotraumaComponent>(uid);
            RemComp<RespiratorComponent>(uid);

            // Nor does it care if you get hungry... maybe?
            RemComp<HungerComponent>(uid);
            RemComp<ThirstComponent>(uid);

            // Don't think husks should be player controlled... however this makes them unclonable aswell... too bad!
            // TODO: Allow the original host to return to a cloned body, shouldn't be hard to do... right?
            if (TryComp<MindComponent>(uid, out var mindComp) && mindComp.Mind != null)
                _gameTicker.OnGhostAttempt(mindComp.Mind, false);

            _bloodstream.SetBloodLossThreshold(uid, 0f);

            RemComp<FactionComponent>(uid); // ? Ok this is dumb but faction system does not remove the enemy factions... so this is nessessary.
            AddComp<FactionComponent>(uid);

            _factionSystem.AddFaction(uid, "SimpleHostile");

            var htn = EnsureComp<HTNComponent>(uid);

            htn.RootTask = "SimpleHostileCompound";

            _npcSystem.SetBlackboard(uid, NPCBlackboard.Owner, uid);

            // Heal the entity fully
            if (TryComp<DamageableComponent>(uid, out var dmgComp))
                _damageSystem.SetAllDamage(uid, dmgComp, 0);

            _npcSystem.WakeNPC(uid);
        }
    }
}
