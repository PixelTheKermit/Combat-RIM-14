using Content.Server.Cargo.Systems;
using Content.Server.Popups;
using Content.Server.Stack;
using Content.Shared.Interaction;
using Content.Server.ManualTurret;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Content.Shared.Stacks;
using Content.Server.Hands.Systems;
using Content.Shared.Verbs;
using Content.Server.Chat.Systems;
using Content.Server.MachineLinking.Events;
using System.Xml.Schema;
using Content.Server.Projectiles.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Content.Shared.Projectiles;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Robust.Shared.Maths;
using System.Linq;
using Robust.Shared.Timing;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Containers;
using Robust.Server.Containers;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Server.Power.Components;
using Content.Shared.Database;
using Content.Shared.FixedPoint;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Physics;
using Serilog;
using Content.Shared.Weapons.Ranged;

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
                    var slotsComp = _entityManager.GetComponent<ItemSlotsComponent>(uid);
                    if (_containerSystem.GetAllContainers(uid).First().ContainedEntities.Count > 0)
                    {
                        var magazine = _containerSystem.GetAllContainers(uid).First().ContainedEntities.First();
                        var bapc = _entityManager.GetComponent<BallisticAmmoProviderComponent>(magazine);

                        if (bapc.UnspawnedCount + bapc.Container.ContainedEntities.Count > 0) // TODO: cry about this fuckin shitcode
                        {
                            var xform = _entityManager.GetComponent<TransformComponent>(uid);
                            if (bapc.Container.ContainedEntities.Count == 0)
                            {
                                bapc.UnspawnedCount -= 1;
                                bapc.Container.Insert(Spawn(bapc.FillProto, xform.MapPosition));
                            }

                            var cartridge = bapc.Container.ContainedEntities.First();
                            if (!_entityManager.GetComponent<CartridgeAmmoComponent>(cartridge).Spent)
                            {
                                _audioSystem.Play(comp.SoundGunshot, Filter.Pvs(uid), uid);
                                var rot = xform.WorldRotation;
                                var bullet = _entityManager.GetComponent<CartridgeAmmoComponent>(cartridge).Prototype;
                                ShootProjectile(Spawn(bullet, xform.MapPosition), rot.ToWorldVec(), uid);
                            }
                            _entityManager.DeleteEntity(cartridge);
                        }
                        else
                        {
                            _audioSystem.Play("/Audio/Weapons/Guns/Empty/empty.ogg", Filter.Pvs(uid), uid);
                        }
                    }
                    else
                    {
                        _audioSystem.Play("/Audio/Weapons/Guns/Empty/empty.ogg", Filter.Pvs(uid), uid);
                    }
                }
                else // I RETURN TO THE CODER MINES!!!!
                {
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
                        _audioSystem.Play("/Audio/Weapons/Guns/Empty/empty.ogg", Filter.Pvs(uid), uid);
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
    }
}
