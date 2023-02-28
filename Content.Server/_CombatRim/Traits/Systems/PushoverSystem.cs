
using Content.Server.Actions.Events;
using Content.Server.Damage.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Prototypes;

namespace Content.Server._CombatRim.Traits.Pushover
{
    public sealed class PushoverSystem : EntitySystem
    {
        [Dependency] private readonly StaminaSystem _staminaSystem = default!;
        [Dependency] private readonly IPrototypeManager _protoManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PushoverComponent, DamageChangedEvent>(OnDamage);
        }

        private void OnDamage(EntityUid uid, PushoverComponent component, DamageChangedEvent args)
        {
            if (!HasComp<StaminaComponent>(uid) || !args.DamageIncreased || args.DamageDelta == null)
                return;

            Logger.Debug("Fuck");

            if (args.DamageDelta.TryGetDamageInGroup(_protoManager.Index<DamageGroupPrototype>("Brute"), out var damage))
            {
                _staminaSystem.TakeStaminaDamage(uid, damage.Float() * 1.75f);
            }
        }
    }
}
