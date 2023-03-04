
using System.Collections.Generic;
using System.Linq;
using Content.Server.Cargo.Systems;
using Content.Server.MachineLinking.Events;
using Content.Server.Stack;
using Content.Shared._Afterlight.Generator;
using Content.Shared.Examine;
using Content.Shared.Maps;
using Content.Shared.Tag;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;

namespace Content.Server._CombatRim.Economy
{
    public sealed class SellPlatformSystem : EntitySystem
    {
        [Dependency] private readonly TagSystem _tagSystem = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly StackSystem _stackSystem = default!;
        [Dependency] private readonly EconomySystem _economySystem = default!;
        [Dependency] private readonly PricingSystem _pricingSystem = default!;
        [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
        [Dependency] private readonly TransformSystem _transformSystem = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SellPlatformComponent, SignalReceivedEvent>(OnSignal);
            SubscribeLocalEvent<SellPlatformComponent, ExaminedEvent>(OnInspect);
        }

        private void OnSignal(EntityUid uid, SellPlatformComponent comp, SignalReceivedEvent args)
        {
            if (args.Port == comp.SellPort)
            {
                if (!TryComp<TransformComponent>(uid, out var xform))
                    return;

                foreach (var otherUid in _entityLookup.GetEntitiesIntersecting(uid, LookupFlags.Dynamic))
                {
                    if (otherUid == uid || (comp.Blacklist != null && comp.Blacklist.IsValid(otherUid)))
                        continue;

                    var cost = _pricingSystem.GetPrice(otherUid)*comp.TaxCut;

                    if (!_economySystem.TryCapitalism((int) cost))
                        continue;

                    var spawned = Spawn(comp.MoneyPrototype, xform.Coordinates);
                    _stackSystem.SetCount(spawned, (int) cost);
                    comp.Contacts.Remove(otherUid);
                    _entityManager.DeleteEntity(otherUid);
                }
            }
        }

        private void OnInspect(EntityUid uid, SellPlatformComponent comp, ExaminedEvent args)
        {
            args.PushText(("Credits: " + _economySystem.credits));
        }
    }
}
