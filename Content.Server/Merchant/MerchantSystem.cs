using Content.Server.Cargo.Systems;
using Content.Server.Popups;
using Content.Server.Stack;
using Content.Shared.Interaction;
using Content.Shared.Merchant;
using Content.Shared.Tag;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Content.Shared.Stacks;
using Content.Server.Hands.Systems;
using Content.Shared.Verbs;
using Content.Server.Chat.Systems;

namespace Content.Server.Merchant
{
    public sealed class MerchantSystem : EntitySystem
    {
        // Dependencies
        [Dependency] private readonly PopupSystem _popup = default!;
        [Dependency] private readonly PricingSystem _pricingSystem = default!;
        [Dependency] private readonly EntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedStackSystem _stackSystem = default!;
        [Dependency] private readonly HandsSystem _handsSystem = default!;
        [Dependency] private readonly ChatSystem _chatSystem = default!;

        public override void Initialize() // VERY IMPORTANT!!!!!!
        {
            base.Initialize();
            SubscribeLocalEvent<MerchantComponent, InteractUsingEvent>(UponInteraction);
            SubscribeLocalEvent<MerchantComponent, InteractHandEvent>(UponEmptyInteraction);
            SubscribeLocalEvent<MerchantComponent, GetVerbsEvent<AlternativeVerb>>(InteractSecond); // todo: learn verbs
        }

        private void InteractSecond(EntityUid uid, MerchantComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (component.Index >= component.Products.Count) // Ensures your purchase isn't out of the list, allows for live changing (?)
                component.Index = 0;
            int price = 100; // A base price, just in case an item doesn't exist
            var objectname = "Unknown (I FUCKED UP!)"; // Tells you if you fucked up lol
            if (_prototypeManager.TryIndex<EntityPrototype>(component.Products[component.Index], out var proto)) 
            {
                var newprice = _pricingSystem.GetEstimatedPrice(proto);
                price = (int) newprice;
                objectname = proto.Name;
            }

            var msg = "Currently Selling " + objectname + " For " + price + " " + component.Currency + "(s)."; // Fuck you localise this yourself bitch
            _chatSystem.TrySendInGameICMessage(uid, msg, InGameICChatType.Speak, true);
        }

        private void UponEmptyInteraction(EntityUid uid, MerchantComponent component, InteractHandEvent args) // Interaction with an empty hand
        {
            component.Index += 1; // Advances the index by 1, this basically selects what to buy
            if (component.Index >= component.Products.Count) // If index exceeds the amount of products, restart
                component.Index = 0;

            var objectname = "Unknown (I FUCKED UP!)"; // Tells you if you fucked up
            int price = 100;
            if (_prototypeManager.TryIndex<EntityPrototype>(component.Products[component.Index], out var proto)) // Checks to ensure you didn't fuck up.
            {
                var newprice = _pricingSystem.GetEstimatedPrice(proto);
                price = (int) newprice;
                objectname = proto.Name;
            }
            var msg = "We're Also Selling " + objectname + " For " + price + " " + component.Currency + "s."; // :godo:
            _chatSystem.TrySendInGameICMessage(uid, msg, InGameICChatType.Speak, true);
        }

        private void UponInteraction(EntityUid uid, MerchantComponent component, InteractUsingEvent args) // Interaction with an item
        {
            if (args.Handled)
                return;

            if (component.Index >= component.Products.Count) // Ensures your purchase isn't out of the list
                component.Index = 0;

            if (_entityManager.TryGetComponent<TagComponent>(args.Used, out var tags) && tags.Tags.TryGetValue(component.Currency, out var tag)) // If the currency is accepted
            {
                int price = 100;
                var objectname = "Unknown (I FUCKED UP!)";
                if (_prototypeManager.TryIndex<EntityPrototype>(component.Products[component.Index], out var proto)) // Check to ensure item is real
                {
                    var newprice = _pricingSystem.GetEstimatedPrice(proto);
                    objectname = proto.Name;
                    price = (int) newprice;
                } // And uh, yeah, I didn't do this correctly. Start crying.

                int amount = 1; // Set as one for currencies without stack sizes
                
                if (_entityManager.TryGetComponent<StackComponent>(args.Used, out var stack)) // Checks if stack size
                    amount = stack.Count;

                if (amount >= price) // Checks if you can buy the item
                {
                    if (stack != null)
                        _stackSystem.SetCount(args.Used, stack.Count - price);
                    else
                        _entityManager.DeleteEntity(args.Used);
                    var productuid = _entityManager.SpawnEntity(component.Products[component.Index], Transform(args.User).Coordinates);
                    _handsSystem.TryPickupAnyHand(args.User, productuid);
                    var msg = "Thank You For Purchasing " + objectname + "!"; // :godo:
                    _chatSystem.TrySendInGameICMessage(uid, msg, InGameICChatType.Speak, true);
                }
                else // Poor ass mf
                {
                    var msg = "You Don't Have Enough " + component.Currency + "!"; // :godo:
                    _chatSystem.TrySendInGameICMessage(uid, msg, InGameICChatType.Speak, true);
                }
            }
            else
            {
                _chatSystem.TrySendInGameICMessage(uid, "Wrong Cash!", InGameICChatType.Speak, true); // For the last time, we do not take bananas as currency!
            }
        }
    }
}
