using Content.Shared.Popups;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Robust.Shared.Player;
using System.Linq;
using Robust.Shared.Prototypes;
using Content.Shared.Tag;

namespace Content.Server.Merchant
{
    public sealed class MerchantSystem : EntitySystem
    {
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly EntityManager _entityManager = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MerchantComponent, InteractUsingEvent>(UponInteraction);
            SubscribeLocalEvent<MerchantComponent, InteractHandEvent>(UponEmptyInteraction);
        }

        private void UponEmptyInteraction(EntityUid uid, MerchantComponent component, InteractHandEvent args)
        {
            if (args.Handled)
                return;
            _popup.PopupEntity("Shop UI is WIP", uid, Filter.Entities(args.User));
        }
        private void UponInteraction(EntityUid uid, MerchantComponent component, InteractUsingEvent args)
        {
            if (args.Handled)
                return;
            if (_entityManager.TryGetComponent<TagComponent>(args.Used, out var tags) && tags.Tags.TryGetValue(component.Currency, out var tag))
            {
                _popup.PopupEntity("Buying stuff is WIP rn!", uid, Filter.Entities(args.User));
            }
            else
            {
                _popup.PopupEntity("That isn't an accepted currency!", uid, Filter.Entities(args.User));
            }
        }
    }
}
