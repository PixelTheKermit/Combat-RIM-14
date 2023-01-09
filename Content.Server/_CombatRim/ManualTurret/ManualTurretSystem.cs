using Content.Server.MachineLinking.Events;
using System.Linq;
using Robust.Shared.Timing;
using Robust.Shared.Player;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Server.Containers;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Server.Power.Components;
using Robust.Shared.Utility;
using Content.Server.Construction;
using Content.Server.Weapons.Ranged.Systems;
using Robust.Shared.Random;
using Content.Shared.Alert;
using Content.Server.CombatMode;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Interaction.Events;
using Content.Shared.Interaction;
using Robust.Server.GameObjects;
using Content.Server.Mind.Components;
using Robust.Shared.Input;
using Content.Server.Mind;
using Content.Shared.Input;
using Robust.Server.Player;
using Robust.Shared.Physics;
using Content.Shared.Movement.Events;
using Content.Server._CombatRim.ControllableMob.Components;
using Content.Server._CombatRim.ControllableMob;

namespace Content.Server._CombatRim.ManualTurret
{
    public sealed class ManualTurretSystem : EntitySystem
    {
        // Dependencies
        [Dependency] protected readonly IRobustRandom Random = default!;
        [Dependency] private readonly EntityManager _entityManager = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
        [Dependency] private readonly ContainerSystem _containerSystem = default!;
        [Dependency] private readonly GunSystem _gunSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
        [Dependency] private readonly ControllableMobSystem _controllableMobSystem = default!;
        [Dependency] private readonly InputSystem _inputSystem = default!;

        public override void Initialize() // VERY IMPORTANT!!!!!!
        {
            base.Initialize();
            SubscribeLocalEvent<ManualTurretComponent, RefreshPartsEvent>(PartsRefresh);
            SubscribeLocalEvent<ManualTurretComponent, UpgradeExamineEvent>(OnUpgradeExamine);
            SubscribeLocalEvent<ManualTurretComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<ManualTurretComponent, UpdateCanMoveEvent>(CanMove);
            SubscribeLocalEvent<ManualTurretComponent, InteractionAttemptEvent>(InteractAttempt);
        }

        private void OnComponentInit(EntityUid uid, ManualTurretComponent comp, ComponentInit args)
        {
            comp.Rotation = Comp<TransformComponent>(uid).WorldRotation;
        }

        private void CanMove(EntityUid uid, ManualTurretComponent comp, UpdateCanMoveEvent args)
        {
            args.Cancel();
        }

        private void InteractAttempt(EntityUid uid, ManualTurretComponent comp, InteractionAttemptEvent args)
        {
            Comp<TransformComponent>(uid).LocalRotation = comp.Rotation;
            args.Cancel();
        }

        /// <summary>
        /// This shit happens every fucking frame
        /// </summary>
        /// <param name="frameTime"></param>
        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            foreach (var (comp, actor, transform) in EntityQuery<ManualTurretComponent, ActorComponent, TransformComponent>())
            {
                transform.LocalRotation = comp.Rotation;

                var session = actor.PlayerSession;

                var input = _inputSystem.GetInputStates(session);

                // This probably shouldn't be done on the server (as it isn't done anywhere else). Too bad!
                if (input.GetState(EngineKeyFunctions.MoveUp) == BoundKeyState.Down)
                    AttemptShoot(comp, session);
                else
                    comp.Firing = false;

                if (input.GetState(EngineKeyFunctions.MoveLeft) == BoundKeyState.Down)
                    comp.Rotation += comp.RotSpeed / 100;
                if (input.GetState(EngineKeyFunctions.MoveRight) == BoundKeyState.Down)
                    comp.Rotation -= comp.RotSpeed / 100;

                if (TryComp<ControllableMobComponent>(comp.Owner, out var contMob) && contMob.CurrentEntityOwning != null
                    && input.GetState(EngineKeyFunctions.MoveDown) == BoundKeyState.Down)
                {
                    Comp<ControllerMobComponent>(contMob.CurrentEntityOwning.Value).Controlling = null;
                    contMob.CurrentDeviceOwning = null;
                    _controllableMobSystem.RevokeControl(contMob.CurrentEntityOwning.Value);
                    contMob.CurrentEntityOwning = null;
                }
            }
        }

        private void AttemptShoot(ManualTurretComponent comp, IPlayerSession session)
        {

            if (!comp.FullAuto && comp.Firing)
                return;

            comp.Firing = true;

            TryShooting(comp, comp.Owner);
        }

        /// <summary>
        /// Changes the efficiency to match the parts inserted into it
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="component"></param>
        /// <param name="args"></param>
        private void PartsRefresh(EntityUid uid, ManualTurretComponent component, RefreshPartsEvent args)
        {
            // Obtain the ratings!
            var firingTimeRating = args.PartRatings[component.MachinePartFiringSpeed];
            var chargeNeededRating = args.PartRatings[component.MachinePartChargeNeeded];
            var accuracyRating = args.PartRatings[component.MachinePartAccuracy];

            // Make the markipliers with some funky math
            component.FireRateMultiplier = MathF.Pow(component.PartRatingFireRateMultiplier, firingTimeRating - 1);
            component.ChargeNeededMultiplier = MathF.Pow(component.PartRatingChargeNeededMultiplier, chargeNeededRating - 1);
            component.AccuracyMultiplier = MathF.Pow(component.PartRatingAccuracyMultiplier, accuracyRating - 1);
            Dirty(component);
        }

        private void OnUpgradeExamine(EntityUid uid, ManualTurretComponent component, UpgradeExamineEvent args)
        {
            args.AddPercentageUpgrade("turret-component-upgrade-speed", 1 / component.FireRateMultiplier);
            args.AddPercentageUpgrade("turret-component-upgrade-charge", 1 / component.ChargeNeededMultiplier);
            args.AddPercentageUpgrade("turret-component-upgrade-accuracy", 1 / component.AccuracyMultiplier);
        }

        /// <summary>
        /// Fired when the turret wants to shoot
        /// </summary>
        /// <param name="comp"></param>
        /// <param name="uid"></param>

        public void TryShooting(ManualTurretComponent comp, EntityUid uid)
        {
            var curTime = _gameTiming.CurTime;
            var fireRate = TimeSpan.FromSeconds(1f / comp.FireRate);

            if (comp.TimeFired + fireRate * comp.FireRateMultiplier > curTime)
                return;

            if (TryComp<BatteryComponent>(uid, out var batteryComp))
            {
                if (batteryComp!.CurrentCharge < comp.FireCost * comp.ChargeNeededMultiplier)
                {
                    _audioSystem.Play(comp.SoundEmpty, Filter.Pvs(uid), uid, true);
                    return;
                }
            }

            var xform = Comp<TransformComponent>(uid);
            var rot = xform.WorldRotation;
            comp.TimeFired = curTime;

            if (!comp.IsBatteryWeapon)
            {
                var container = _containerSystem.GetContainer(uid, "turret_mag");

                if (container.ContainedEntities.Count == 0)
                {
                    _audioSystem.Play(comp.SoundEmpty, Filter.Pvs(uid), uid, true);
                    return;
                }

                var magazine = container.ContainedEntities.First(); // There's probably a better way of doing this but it's good for now
                var ammoComp = Comp<BallisticAmmoProviderComponent>(magazine); // This prays on the fact that the thing in the gun is a mag.

                if (ammoComp.UnspawnedCount + ammoComp.Entities.Count < 0)
                {
                    _audioSystem.Play(comp.SoundEmpty, Filter.Pvs(uid), uid, true);
                    return;
                }

                var cartridge = GetCartridge(ammoComp, xform);
                var angle = GetRecoilAngle(_gameTiming.CurTime, comp, rot);

                if (TryComp<CartridgeAmmoComponent>(cartridge, out var cartridgeComp))
                {
                    if (cartridgeComp.Spent) // a wasted bullet is a useless one
                    {
                        _entityManager.DeleteEntity(cartridge);
                        _audioSystem.Play(comp.SoundEmpty, Filter.Pvs(uid), uid, true);
                        return;
                    }

                    _audioSystem.Play(comp.SoundGunshot, Filter.Pvs(uid), uid, true);

                    var bullet = cartridgeComp.Prototype;
                    if (cartridgeComp.Count > 1) // For shotgun-like bullets
                    {
                        var angles = LinearSpread(rot - cartridgeComp.Spread / 2,
                            rot + cartridgeComp.Spread / 2, cartridgeComp.Count);

                        for (var i = 0; i < cartridgeComp.Count; i++)
                        {
                            _gunSystem.ShootProjectile(Spawn(bullet, xform.MapPosition), angles[i].ToWorldVec(), angles[i].ToWorldVec() * 10, uid);
                        }
                    }
                    else
                        _gunSystem.ShootProjectile(Spawn(bullet, xform.MapPosition), angle.ToWorldVec(), angle.ToWorldVec() * 10, uid);

                    if (batteryComp != null)
                        batteryComp.CurrentCharge -= comp.FireCost * comp.ChargeNeededMultiplier;

                    _entityManager.DeleteEntity(cartridge); // This is better for performance, for both the client and the server.
                }
                else // This is for fun, mostly. Could see some good use later maybe (Spear launcher, anyone?)
                {
                    _audioSystem.Play(comp.SoundGunshot, Filter.Pvs(uid), uid, true);
                    var bullet = cartridge;
                    ammoComp.Entities.Remove(cartridge);
                }
                Dirty(magazine);
                UpdateAppearance(ammoComp);
            }
            else // Does not support hitscan or shotgun-like patterns YET
            {
                var angle = GetRecoilAngle(_gameTiming.CurTime, comp, rot);
                var projComp = Comp<ProjectileBatteryAmmoProviderComponent>(uid);

                if (batteryComp != null)
                    batteryComp.CurrentCharge -= comp.FireCost * comp.ChargeNeededMultiplier;

                var bullet = projComp.Prototype;
                _audioSystem.Play(comp.SoundGunshot, Filter.Pvs(uid), uid, true);
                _gunSystem.ShootProjectile(Spawn(bullet, xform.MapPosition), angle.ToWorldVec(), angle.ToWorldVec() * 10, uid);
            }
        }

        /// <summary>
        /// Gets the cartridge of a gun. Moved to a function because it's cleaner
        /// </summary>
        /// <param name="ammoComp">The ballistic ammo component.</param>
        /// <param name="xform">The transform component.</param>
        /// <returns></returns>
        private EntityUid GetCartridge(BallisticAmmoProviderComponent ammoComp, TransformComponent xform)
        {
            if (ammoComp.Entities.Count == 0)
            {
                ammoComp.UnspawnedCount -= 1;
                return Spawn(ammoComp.FillProto, xform.MapPosition);
            }
            else
            {
                var cartridge = ammoComp.Entities.Last();
                ammoComp.Entities.Remove(cartridge);
                Dirty(ammoComp);
                return cartridge;
            }
        }

        private Angle GetRecoilAngle(TimeSpan curTime, ManualTurretComponent component, Angle direction)
        {
            var timeSinceLastFire = (curTime - component.TimeFired).TotalSeconds;
            var newTheta = MathHelper.Clamp(component.CurrentAngle.Theta + component.AngleIncrease.Theta * component.AccuracyMultiplier - component.AngleDecay.Theta / component.AccuracyMultiplier * timeSinceLastFire, component.MinAngle.Theta * component.AccuracyMultiplier, component.MaxAngle.Theta * component.AccuracyMultiplier);
            component.CurrentAngle = new Angle(newTheta);

            var random = Random.NextFloat(-0.5f, 0.5f);
            var spread = component.CurrentAngle.Theta * random;
            var angle = new Angle(direction.Theta + component.CurrentAngle.Theta * random);
            DebugTools.Assert(spread <= component.MaxAngle.Theta);
            return angle;
        }

        private Angle[] LinearSpread(Angle start, Angle end, int intervals)
        {
            var angles = new Angle[intervals];
            DebugTools.Assert(intervals > 1);

            for (var i = 0; i <= intervals - 1; i++)
            {
                angles[i] = new Angle(start + (end - start) * i / (intervals - 1));
            }

            return angles;
        }

        private void UpdateAppearance(BallisticAmmoProviderComponent component)
        {
            if (!_gameTiming.IsFirstTimePredicted || !TryComp<AppearanceComponent>(component.Owner, out var appearance))
                return;

            _appearanceSystem.SetData(appearance.Owner, AmmoVisuals.AmmoCount, component.UnspawnedCount + component.Entities.Count, appearance);
            _appearanceSystem.SetData(appearance.Owner, AmmoVisuals.AmmoMax, component.Capacity, appearance);
        }
    }
}
