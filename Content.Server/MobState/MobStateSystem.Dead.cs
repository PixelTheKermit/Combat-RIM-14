using Content.Shared.Alert;
using Content.Shared.MobState.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Robust.Shared.Player;

namespace Content.Server.MobState;

public sealed partial class MobStateSystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void EnterDeadState(EntityUid uid)
    {
        base.EnterDeadState(uid);

        var component = Comp<MobStateComponent>(uid);

        Alerts.ShowAlert(uid, AlertType.HumanDead);

        if (HasComp<StatusEffectsComponent>(uid))
        {
            Status.TryRemoveStatusEffect(uid, "Stun");
        }
    }
}
