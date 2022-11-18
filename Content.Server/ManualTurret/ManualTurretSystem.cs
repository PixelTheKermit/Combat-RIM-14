using Content.Server.MachineLinking.Events;
using Content.Server.Projectiles.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Content.Shared.Projectiles;
using System.Linq;
using Robust.Shared.Timing;
using Robust.Shared.Player;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Server.Containers;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Server.Power.Components;
using Robust.Shared.Utility;
using Robust.Shared.Containers;

namespace Content.Server.ManualTurret
{
    public sealed class ManualTurretSystem : EntitySystem
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
            SubscribeLocalEvent<ManualTurretComponent, SignalReceivedEvent>(Signal);
        }

        /// <summary>
        /// This shit happens every fucking frame
        /// </summary>
        /// <param name="frameTime"></param>
        public override void Update(float frameTime)
        {
            base.Update(frameTime);
            foreach (var (turret, xform) in EntityQuery<ManualTurretComponent, TransformComponent>())
            {
                xform.LocalRotation += turret.CurRotSpeed/100;
                if (turret.Firing)
                {
                    TryShooting(turret, turret.Owner);
                }
            }
        }

        /// <summary>
        /// For any signal events
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="component"></param>
        /// <param name="args"></param>
        private void Signal(EntityUid uid, ManualTurretComponent component, SignalReceivedEvent args)
        {
            if (args.Port == component.ClockwiseRot)
            {
                component.CurRotSpeed = component.RotSpeed;
            }
            if (args.Port == component.AntiClockwiseRot)
            {
                component.CurRotSpeed = -component.RotSpeed;
            }
            if (args.Port == component.StopRot)
            {
                component.CurRotSpeed = 0;
            }
            if (args.Port == component.TurretSemi)
            {
                TryShooting(component, uid);
            }
            if (args.Port == component.TurretAutoToggle)
            {
                component.Firing = !component.Firing;
            }
            if (args.Port == component.TurretAutoOn)
            {
                component.Firing = true;
            }
            if (args.Port == component.TurretAutoOff)
            {
                component.Firing = false;
            }
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

            if (comp.TimeFired + fireRate <= curTime)
            {
                comp.TimeFired = curTime;
                if (!comp.IsBatteryWeapon)
                {
                    var container = _containerSystem.GetAllContainers(uid).First();
                    if (container.ContainedEntities.Count > 0)
                    {
                        var magazine = container.ContainedEntities.First(); // There's probably a better way of doing this but it's good for now
                        var ammoComp = _entityManager.GetComponent<BallisticAmmoProviderComponent>(magazine); // This prays on the fact that the thing you're trying to insert is a mag.

                        if (ammoComp.UnspawnedCount + ammoComp.Entities.Count > 0) // TODO: cry about this fuckin shitcode
                        {
                            // Some positional data, so we know where to shoot.
                            var xform = _entityManager.GetComponent<TransformComponent>(uid);
                            var rot = xform.WorldRotation;

                            var cartridge = EntityUid.Invalid;

                            if (ammoComp.Entities.Count == 0)
                            {
                                ammoComp.UnspawnedCount -= 1;
                                cartridge = Spawn(ammoComp.FillProto, xform.MapPosition);
                            }
                            else
                                cartridge = ammoComp.Entities.Last(); // Is it better to do last or first? fuck it we're doing last.

                            if (_entityManager.TryGetComponent<CartridgeAmmoComponent>(cartridge, out var cartridgeComp))
                            {
                                if (!cartridgeComp.Spent) // a wasted bullet is a useless one
                                {
                                    _audioSystem.Play(comp.SoundGunshot, Filter.Pvs(uid), uid);

                                    var bullet = cartridgeComp.Prototype;
                                    if (cartridgeComp.Count > 1) // For shotgun-like bullets
                                    {
                                        var angles = LinearSpread(rot - cartridgeComp.Spread / 2,
                                            rot + cartridgeComp.Spread / 2, cartridgeComp.Count);

                                        for (var i = 0; i < cartridgeComp.Count; i++)
                                        {
                                            ShootProjectile(Spawn(bullet, xform.MapPosition), angles[i].ToWorldVec(), uid);
                                        }
                                    }
                                    else
                                        ShootProjectile(Spawn(bullet, xform.MapPosition), rot.ToWorldVec(), uid);
                                    _entityManager.DeleteEntity(cartridge); // This is better for performance, for both the client and the server.
                                    //Dirty(cartridge);
                                    //cartridgeComp.Spent = true;
                                    //_appearanceSystem.SetData(cartridge, AmmoVisuals.Spent, true);
                                }
                                if (ammoComp.Entities.Count > 0)
                                    ammoComp.Entities.Remove(cartridge);
                            }
                            else // This is for fun, mostly. Could see some good use later maybe (Spear launcher, anyone?)
                            {
                                _audioSystem.Play(comp.SoundGunshot, Filter.Pvs(uid), uid);
                                var bullet = cartridge;
                                ammoComp.Entities.Remove(cartridge);
                                ShootProjectile(bullet, rot.ToWorldVec(), uid);
                            }
                            Dirty(magazine);
                            UpdateAppearance(ammoComp);
                        }
                        else
                        {
                            _audioSystem.Play(comp.SoundEmpty, Filter.Pvs(uid), uid);
                        }
                    }
                    else
                    {
                        _audioSystem.Play(comp.SoundEmpty, Filter.Pvs(uid), uid);
                    }
                }
                else // Does not support hitscan or shotgun-like patterns YET
                { // This was actually easier to implement, says a lot about containers.
                    var batteryComp = _entityManager.GetComponent<BatteryComponent>(uid); 
                    var hitscanProjComp = _entityManager.GetComponent<ProjectileBatteryAmmoProviderComponent>(uid);

                    if (batteryComp.CurrentCharge >= hitscanProjComp.FireCost) 
                    {
                        batteryComp.CurrentCharge -= hitscanProjComp.FireCost;
                        var xform = _entityManager.GetComponent<TransformComponent>(uid);
                        _audioSystem.Play(comp.SoundGunshot, Filter.Pvs(uid), uid);
                        var rot = xform.WorldRotation;
                        var bullet = hitscanProjComp.Prototype;
                        ShootProjectile(Spawn(bullet, xform.MapPosition), rot.ToWorldVec(), uid);
                    }
                    else
                    {
                        _audioSystem.Play(comp.SoundEmpty, Filter.Pvs(uid), uid);
                    }
                }
            }
        }


        /// <summary>
        /// Swiped from gun script
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="direction"></param>
        /// <param name="user"></param>
        /// <param name="speed"></param>
        public void ShootProjectile(EntityUid uid, Vector2 direction, EntityUid? user = null, float speed = 20f)
        {
            var physics = EnsureComp<PhysicsComponent>(uid);
            physics.BodyStatus = BodyStatus.InAir;
            _physicsSystem.SetLinearVelocity(physics, direction.Normalized * speed);

            if (user != null)
            {
                var projectile = EnsureComp<ProjectileComponent>(uid);
                _projectilesSystem.SetShooter(projectile, user.Value);
            }

            Transform(uid).WorldRotation = direction.ToWorldAngle();
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
