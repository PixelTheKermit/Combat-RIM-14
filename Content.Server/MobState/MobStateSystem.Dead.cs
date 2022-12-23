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

        _popupSystem.PopupEntity(Loc.GetString(component.DeathMessage, ("mob", Comp<MetaDataComponent>(uid).EntityName)), uid,PopupType.SmallCaution);

        Alerts.ShowAlert(uid, AlertType.HumanDead);

        if (HasComp<StatusEffectsComponent>(uid))
        {
            Status.TryRemoveStatusEffect(uid, "Stun");
        }
    }
}
