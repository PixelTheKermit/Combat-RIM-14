using Content.Server.Chat;
using Content.Server.Popups;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;

namespace Content.Server._CombatRim.DeathMessage
{
    public sealed class DeathMessageSystem : EntitySystem
    {
        [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly SuicideSystem _suicideSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<DeathMessageComponent, SuicideEvent>(OnSuicide);
            SubscribeLocalEvent<DeathMessageComponent, MobStateChangedEvent>(OnMobStateChanged);
        }

        private void OnSuicide(EntityUid uid, DeathMessageComponent component, SuicideEvent args)
        {
            if (args.AttemptBlocked)
                return;

            component.HadSuicided = true;
        }

        private void OnMobStateChanged(EntityUid uid, DeathMessageComponent component, MobStateChangedEvent args)
        {

            if (!_mobStateSystem.IsDead(uid))
            {
                component.HadSuicided = false;
                return;
            }

            if (component.HadSuicided)
                return;

            _popupSystem.PopupEntity(Loc.GetString(component.Message, ("mob", uid)), uid, PopupType.SmallCaution);
        }

    }
}
