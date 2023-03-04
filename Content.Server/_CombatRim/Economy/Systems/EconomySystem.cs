
using System.Diagnostics;
using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Events;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CombatRim.Economy
{
    /// <summary>
    /// This will be the basis of the economy rework of CR.
    /// TODO: A fuckton
    /// </summary>
    public sealed class EconomySystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;
        [Dependency] private readonly IPrototypeManager _protoManager = default!;

        public float credits = 0f;

        private TimeSpan? nextEconomicCrisis;

        private string announcer = "Bank of the Death Sector";

        public override void Initialize() // TODO: Once there is an event for checking if an item has been brought from a store, make it add to the economy
        {
            base.Initialize();

            SubscribeLocalEvent<RoundStartingEvent>(RoundStarted);
            SubscribeLocalEvent<RoundRestartCleanupEvent>(RoundEnded);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var curTime = _gameTiming.CurTime;

            if (curTime > nextEconomicCrisis)
            {
                nextEconomicCrisis = _gameTiming.CurTime + TimeSpan.FromMinutes(_random.Next(10, 25));
                DoRandomEvent();
            }
        }

        private void RoundStarted(RoundStartingEvent args)
        {
            credits = _random.Next(1, 3)*10000;
            Logger.Info("Credits: " + credits);
            nextEconomicCrisis = _gameTiming.CurTime + TimeSpan.FromMinutes(_random.Next(10, 25));
        }

        private void RoundEnded(RoundRestartCleanupEvent args)
        {
            Logger.Info("Ending credits: " + credits);
            nextEconomicCrisis = null;
        }

        public void DoRandomEvent()
        {
            var allEvents = _protoManager.EnumeratePrototypes<EconomicEventPrototype>().ToArray();

            if (!allEvents.Any())
                return;

            var randEvent = _random.Pick(allEvents);

            credits += randEvent.AddedOn;
            credits *= randEvent.Multiplier;

            credits = (int) credits;

            if (credits < 0)
                credits = 0;

            _chatSystem.DispatchGlobalAnnouncement(Loc.GetString(randEvent.Text), announcer, true, null, Color.Violet);
        }

    /// <summary>
    /// Remove from or contribute to the economy!
    /// WE DO CAPITALISM HERE ON THE DEATH SECTORS!
    /// </summary>
    /// <param name="price">Positive if removing from economy, negative if contributing. Confusing I know. You may ask why, it's because I want to fuck with you.</param>
    /// <returns></returns>
        public bool TryCapitalism(int price)
        {
            if (credits >= price)
            {
                credits -= price;
                return true;
            }
            return false;
        }
    }
}
