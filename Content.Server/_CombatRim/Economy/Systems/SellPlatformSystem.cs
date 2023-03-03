
using System.Collections.Generic;
using System.Linq;
using Content.Server.Cargo.Systems;
using Content.Server.MachineLinking.Events;
using Content.Server.Stack;
using Content.Shared.Examine;
using Content.Shared.Tag;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Physics.Events;

namespace Content.Server._CombatRim.Economy
{
    public sealed class SellPlatformSystem : EntitySystem
    {
        [Dependency] private readonly TagSystem _tagSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly StackSystem _stackSystem = default!;
        [Dependency] private readonly EconomySystem _economySystem = default!;
        [Dependency] private readonly PricingSystem _pricingSystem = default!;


        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SellPlatformComponent, StartCollideEvent>(OnCollide);
            SubscribeLocalEvent<SellPlatformComponent, EndCollideEvent>(OnNoCollide);
            SubscribeLocalEvent<SellPlatformComponent, SignalReceivedEvent>(OnSignal);
            SubscribeLocalEvent<SellPlatformComponent, ExaminedEvent>(OnInspect);
        }

        private void OnCollide(EntityUid uid, SellPlatformComponent comp, ref StartCollideEvent args)
        {
            var otherUid = args.OtherFixture.Body.Owner; // ! IF IT'S FUCKING DEPRECATED ALLOW ME TO ACCESS THE OWNER ANOTHER WAY!

            if (comp.Blacklist != null && comp.Blacklist.IsValid(otherUid))
                return;

            comp.Contacts.Add(otherUid);
        }

        private void OnNoCollide(EntityUid uid, SellPlatformComponent comp, ref EndCollideEvent args)
        {
            comp.Contacts.Remove(args.OtherFixture.Body.Owner);
        }

        private void OnSignal(EntityUid uid, SellPlatformComponent comp, SignalReceivedEvent args)
        {
            if (args.Port == comp.SellPort)
            {
                foreach (var otherUid in comp.Contacts)
                {
                    var cost = _pricingSystem.GetPrice(otherUid)*comp.TaxCut;
                    if (_economySystem.TryCapitalism((int) cost))
                    {
                        var spawned = Spawn(comp.MoneyPrototype, Comp<TransformComponent>(uid).Coordinates);
                        _stackSystem.SetCount(spawned, (int) cost);
                        comp.Contacts.Remove(otherUid);
                        _entityManager.DeleteEntity(otherUid);
                    }
                }
            }
        }

        private void OnInspect(EntityUid uid, SellPlatformComponent comp, ExaminedEvent args)
        {
            args.PushText(("Credits: " + _economySystem.credits));
        }
    }
}
