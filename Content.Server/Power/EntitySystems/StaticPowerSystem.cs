using Content.Server._OuterRim.Generator;
using Content.Server.Power.Components;

namespace Content.Server.Power.EntitySystems;

public static class StaticPowerSystem
{
    // Using this makes the call shorter.
    // ReSharper disable once UnusedParameter.Global
    public static bool IsPowered(this EntitySystem system, EntityUid uid, IEntityManager entManager, ApcPowerReceiverComponent? receiver = null)
    {

        if (receiver == null && !entManager.TryGetComponent(uid, out receiver))
            return (entManager.TryGetComponent<GasPowerProviderComponent>(uid, out var gasPower) && gasPower.Powered);

        return receiver.Powered;
    }
}
