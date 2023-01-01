using Robust.Shared.Physics.Systems;
using Content.Shared.Projectiles;
using Robust.Shared.Timing;
using Robust.Server.Containers;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Server.Body.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Content.Server.Atmos.Components;

namespace Content.Server._CombatRim.GasFly
{
    public sealed class GasFlySystem : EntitySystem
    {
        // Dependencies
        [Dependency] private readonly EntityManager _entityManager = default!;
        [Dependency] private readonly SharedPhysicsSystem _physicsSystem = default!;
        [Dependency] private readonly SharedProjectileSystem _projectilesSystem = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
        [Dependency] private readonly ContainerSystem _containerSystem = default!;
        [Dependency] private readonly SharedGunSystem _gunSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;

        public override void Initialize() // VERY IMPORTANT!!!!!!
        {
            base.Initialize();
            SubscribeLocalEvent<GasFlyComponent, CanWeightlessMoveEvent>(WeightlessMove);
        }

        /// <summary>
        /// Checks if the entity has enough GAS GAS GAS to move in space
        /// </summary>
        /// <param name="frameTime"></param>
        private void WeightlessMove(EntityUid uid, GasFlyComponent comp, ref CanWeightlessMoveEvent args)
        {
            if (_entityManager.TryGetComponent<InternalsComponent>(uid, out var internalComp))
            {
                if (internalComp.GasTankEntity.HasValue &&
                    _entityManager.GetComponent<GasTankComponent>(internalComp.GasTankEntity.Value).OutputPressure >= comp.GasRequired)
                {
                    args.CanMove = true;
                }
            }

        }
    }
}
