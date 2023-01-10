using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Chat;
using Content.Server.MachineLinking.System;
using Content.Server.MobState;
using Content.Server.Nutrition.Components;
using Content.Server.Nutrition.EntitySystems;
using Content.Server.Popups;
using Content.Server.Stack;
using Content.Shared.Audio;
using Content.Shared.Body.Components;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.MachineLinking;
using Content.Shared.MobState;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using System.Linq;
using System.Security.AccessControl;

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

            _foodSystem.TryFeed(args.User, args.User, foodComp);
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

            _foodSystem.TryFeed(args.User, args.Target.Value, foodComp);
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
