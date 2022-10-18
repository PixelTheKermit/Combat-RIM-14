using System.Linq;
using Content.Server.Cargo.Systems;
using Content.Server.Popups;
using Content.Server.Stack;
using Content.Shared.Interaction;
using Content.Shared.Merchant;
using Content.Shared.Tag;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Content.Shared.Stacks;
using Content.Server.Hands;
using Content.Server.Hands.Systems;
using Content.Server.Coordinates;
using Robust.Shared.Map;
using Content.Shared.OuterRim.Generator;
using Content.Shared.Verbs;

namespace Content.Server.Merchant
{
    public sealed class MerchantSystem : EntitySystem
    {
        [Dependency] private readonly PopupSystem _popup = default!;
        [Dependency] private readonly PricingSystem _pricingSystem = default!;
        [Dependency] private readonly EntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedStackSystem _stackSystem = default!;
        [Dependency] private readonly HandsSystem _handsSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MerchantComponent, InteractUsingEvent>(UponInteraction);
            SubscribeLocalEvent<MerchantComponent, InteractHandEvent>(UponEmptyInteraction);
            SubscribeLocalEvent<MerchantComponent, ActivateInWorldEvent>(InteractSecond);
        }

        // TODO: Do this later.

        private void InteractSecond(EntityUid uid, MerchantComponent component, ActivateInWorldEvent args)
        {
            int price = 100;
            if (_prototypeManager.TryIndex<EntityPrototype>(component.Products[component.Index], out var proto))
            {
                var newprice = _pricingSystem.GetEstimatedPrice(proto);
                price = (int) newprice;
            }
            _popup.PopupEntity("The merchant is selling" + component.Products[component.Index] + " for " + price +" "+ component.Currency + "s.", uid, Filter.Entities(args.User));
        }

        private void UponEmptyInteraction(EntityUid uid, MerchantComponent component, InteractHandEvent args)
        {
            component.Index += 1;
            if (component.Index >= component.Products.Count)
                component.Index = 0;
            int price = 100;
            if (_prototypeManager.TryIndex<EntityPrototype>(component.Products[component.Index], out var proto))
            {
                var newprice = _pricingSystem.GetEstimatedPrice(proto);
                price = (int) newprice;
            }
            _popup.PopupEntity("The merchant shows you " + component.Products[component.Index] + " for " + price +" " + component.Currency + "s!", uid, Filter.Entities(args.User));
        }

        private void UponInteraction(EntityUid uid, MerchantComponent component, InteractUsingEvent args)
        {
            if (args.Handled)
                return;
            if (component.Index >= component.Products.Count)
                component.Index = 0;
            if (_entityManager.TryGetComponent<TagComponent>(args.Used, out var tags) && tags.Tags.TryGetValue(component.Currency, out var tag))
            {
                int price = 100;
                if (_prototypeManager.TryIndex<EntityPrototype>(component.Products[component.Index], out var proto))
                {
                    var newprice = _pricingSystem.GetEstimatedPrice(proto);
                    price = (int) newprice;
                }
                int amount = 1;
                
                if (_entityManager.TryGetComponent<StackComponent>(args.Used, out var stack))
                    amount = stack.Count;
                if (amount >= price)
                {
                    if (stack != null)
                        _stackSystem.SetCount(args.Used, stack.Count - price);
                    else
                        _entityManager.DeleteEntity(args.Used);
                    var productuid = _entityManager.SpawnEntity(component.Products[component.Index], Transform(args.User).Coordinates);
                    _handsSystem.TryPickupAnyHand(args.User, productuid);
                    _popup.PopupEntity("Purchased " + component.Products[component.Index] + "!", uid, Filter.Entities(args.User));
                }
                else
                    _popup.PopupEntity("You don't have enough "+component.Currency+"!", uid, Filter.Entities(args.User));
            }
            else
            {
                _popup.PopupEntity("That isn't an accepted currency!", uid, Filter.Entities(args.User));
            }
        }
    }
}
