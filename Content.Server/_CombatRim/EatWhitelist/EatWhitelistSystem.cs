using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Nutrition.Components;
using Content.Server.Nutrition.EntitySystems;
using Content.Shared.Body.Components;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Stacks;
using System.Linq;

namespace Content.Server._CombatRim.EatWhitelist
{
    public sealed class EatWhitelistSystem : EntitySystem
    {
        [Dependency] private readonly FoodSystem _foodSystem = default!;
        [Dependency] private readonly BodySystem _bodySystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<EatWhitelistComponent, UseInHandEvent>(OnUseAttempt);
            SubscribeLocalEvent<EatWhitelistComponent, AfterInteractEvent>(OnFeedAttempt);
        }

        private void OnUseAttempt(EntityUid uid, EatWhitelistComponent comp, UseInHandEvent args)
        {
            if (!TryComp<FoodComponent>(uid, out var foodComp))
                return;

            args.Handled = true;

            if (TryComp<StackComponent>(uid, out var stack) && stack.Count > 1)
                return;

            if (!TryComp<BodyComponent>(args.User, out var body))
                return;

            var stomach = GetStomach(args.User, body);

            if (stomach == null || !comp.Stomachs.Any(x => x == Comp<MetaDataComponent>(stomach.Owner).EntityPrototype!.ID))
                return;

            _foodSystem.TryFeed(args.User, args.User, uid, foodComp);
        }

        private void OnFeedAttempt(EntityUid uid, EatWhitelistComponent comp, AfterInteractEvent args)
        {

            if (!TryComp<FoodComponent>(uid, out var foodComp))
                return;

            args.Handled = true;

            if (TryComp<StackComponent>(uid, out var stack) && stack.Count > 1)
                return;

            if (args.Target == null || !args.CanReach || !TryComp<BodyComponent>(args.Target.Value, out var body))
                return;

            var stomach = GetStomach(args.Target.Value, body);

            if (stomach == null || !comp.Stomachs.Any(x => x == Comp<MetaDataComponent>(stomach.Owner).EntityPrototype!.ID))
                return;

            _foodSystem.TryFeed(args.User, args.Target.Value, uid, foodComp);
        }

        private StomachComponent? GetStomach(EntityUid uid, BodyComponent body)
        {
            foreach (var (component, _) in _bodySystem.GetBodyOrganComponents<StomachComponent>(uid))
            {
                return component;
            }

            return null;
        }

    }
}
