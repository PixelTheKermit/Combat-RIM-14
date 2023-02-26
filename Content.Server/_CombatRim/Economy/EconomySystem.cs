
using System.Diagnostics;
using Content.Server.GameTicking.Events;
using Robust.Shared.Random;

namespace Content.Server._CombatRim.Economy
{
    /// <summary>
    /// This will be the basis of the economy rework of CR.
    /// TODO: A fuckton
    /// </summary>
    public sealed class EconomySystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;

        public float credits = 0f;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RoundStartingEvent>(RoundStarted);
        }

        private void RoundStarted(RoundStartingEvent args)
        {
            credits = _random.Next(10, 25)*100000;

            Logger.Info("Credits: " + credits);
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
