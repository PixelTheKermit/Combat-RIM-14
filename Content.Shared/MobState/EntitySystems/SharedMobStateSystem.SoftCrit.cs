using Content.Shared.Alert;
using Content.Shared.FixedPoint;

namespace Content.Shared.MobState.EntitySystems;

public abstract partial class SharedMobStateSystem
{
    public virtual void EnterSoftCritState(EntityUid uid)
    {
        Alerts.ShowAlert(uid, AlertType.HumanCrit);
        _standing.Down(uid);
        _appearance.SetData(uid, DamageStateVisuals.State, DamageState.SoftCrit);
    }

    public virtual void ExitSoftCritState(EntityUid uid)
    {
        _standing.Stand(uid);
    }

    public virtual void UpdateSoftCritState(EntityUid entity, FixedPoint2 threshold) { }
}
