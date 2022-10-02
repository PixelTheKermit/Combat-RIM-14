using Content.Shared.Alert;
using Content.Shared.FixedPoint;

namespace Content.Shared.MobState.EntitySystems;

public abstract partial class SharedMobStateSystem
{
    public virtual void EnterSoftCritState(EntityUid uid)
    {
        Alerts.ShowAlert(uid, AlertType.HumanCrit);

        Standing.Down(uid);

        if (TryComp<AppearanceComponent>(uid, out var appearance))
        {
            appearance.SetData(DamageStateVisuals.State, DamageState.SoftCrit);
        }
    }

    public virtual void ExitSoftCritState(EntityUid uid)
    {
        Standing.Stand(uid);
    }

    public virtual void UpdateSoftCritState(EntityUid entity, FixedPoint2 threshold) { }
}
