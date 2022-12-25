using Content.Server.Cargo.Systems;
using Content.Server.Popups;
using Content.Shared.Interaction;
using Robust.Shared.Prototypes;
using Content.Server.Hands.Systems;
using Content.Shared.Stacks;
using Content.Shared.Tag;
using Content.Server.Stack;

namespace Content.Server.Merchant.Sell
{
    public sealed class MerchantSellSystem : EntitySystem
    {
        // Dependencies
        [Dependency] private readonly PopupSystem _popup = default!;
        [Dependency] private readonly PricingSystem _pricingSystem = default!;
        [Dependency] private readonly EntityManager _entityManager = default!;
        [Dependency] private readonly TagSystem _tagSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly StackSystem _stackSystem = default!;
        [Dependency] private readonly HandsSystem _handsSystem = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;

        public override void Initialize() // VERY IMPORTANT!!!!!!
        {
            base.Initialize();
            SubscribeLocalEvent<MerchantSellComponent, InteractUsingEvent>(UponInteraction);
            SubscribeLocalEvent<MerchantSellComponent, InteractHandEvent>(EmptyHandInteraction);
        }

        /// <summary>
        /// Interaction with an item, this initiates the selling process
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="component"></param>
        /// <param name="args"></param>
        private void UponInteraction(EntityUid uid, MerchantSellComponent component, InteractUsingEvent args) // Interaction with an item
        {
            if (args.Handled)
                return;

            if (_tagSystem.HasAnyTag(args.Used, component.Blacklist))
            {
                _popupSystem.PopupEntity("You cannot sell this!", uid, args.User);
                return;
            }

            var moneyProto = _prototypeManager.Index<EntityPrototype>(component.CashPrototype);

            var isProtoStackable = moneyProto.TryGetComponent<StackComponent>(out var protoStackComp);

            if (!isProtoStackable)
            {
                _popupSystem.PopupEntity("The coder fucked up.", uid, args.User);
                return;
            }

            var isStackable = _entityManager.TryGetComponent<StackComponent>(args.Used, out var stackComp);

            if (isStackable && protoStackComp!.StackTypeId == stackComp!.StackTypeId) // This is probably a shit way of doing this, too bad!
            {
                _popupSystem.PopupEntity("You cannot sell money!", uid, args.User);
                return;
            }

            var price = _pricingSystem.GetPrice(args.Used) * component.Tax;

            if ((int) price <= 0)
            {
                _popupSystem.PopupEntity("This item has no value.", uid, args.User);
                return;
            }

            if (component.Funds >= price)
            {
                component.Funds -= (int) price;
                var moneyEntity = Spawn(component.CashPrototype, _entityManager.GetComponent<TransformComponent>(args.User).Coordinates);
                _stackSystem.SetCount(moneyEntity, (int) price);
                _entityManager.DeleteEntity(args.Used);
                _handsSystem.TryPickupAnyHand(args.User, moneyEntity);
            }
            else
                _popupSystem.PopupEntity("It doesn't have enough funds.", uid, args.User);

            args.Handled = true;
        }

        /// <summary>
        /// Interaction with an item, this initiates the selling process
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="component"></param>
        /// <param name="args"></param>
        private void EmptyHandInteraction(EntityUid uid, MerchantSellComponent component, InteractHandEvent args)
        {
            if (args.Handled)
                return;

            _popupSystem.PopupEntity("Funds: " + component.Funds, uid, args.User);
            args.Handled = true;
        }
    }
}
