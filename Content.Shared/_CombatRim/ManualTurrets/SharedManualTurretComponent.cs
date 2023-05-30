
using Content.Shared.Construction.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._CombatRim.ManualTurret;

[RegisterComponent, NetworkedComponent]
public sealed class ManualTurretComponent : Component
{
    /// <summary>
    /// The max speed of the turret.
    /// </summary>
    [DataField("rotSpeed")]
    public float RotSpeed = 5f;

    /// <summary>
    /// The desired rotation of the turret.
    /// </summary>
    public Angle NewRot;

    /// <summary>
    /// Is the turret a dadadadadada or a pow....... pow?
    /// </summary>
    [DataField("fullAuto")]
    public bool FullAuto = false;

    /// <summary>
    /// Not the rate it gets burnt at
    /// </summary>
    [DataField("fireRate")]
    public float FireRate = 1.5f;

    /// <summary>
    /// The last time the turret has been fired... How is it gonna get a job now?
    /// </summary>
    public TimeSpan TimeFired = TimeSpan.Zero;

    /// <summary>
    /// Are you allowed to stand in front of the barrel?
    /// </summary>
    public bool Firing = false;

    /// <summary>
    /// Does it use standard cartridges or pure power?
    /// </summary>
    [DataField("isBatteryWeapon")]
    public bool IsBatteryWeapon = false;

    /// <summary>
    /// How much power will the turret need?
    /// </summary>
    [DataField("fireCost")]
    public float FireCost = 0f;

    /// <summary>
    /// Current angle of spread
    /// </summary>
    [DataField("currentAngle")]
    public Angle CurrentAngle;

    /// <summary>
    /// The increase of spread per shot.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("angleIncrease")]
    public Angle AngleIncrease = Angle.FromDegrees(0.5);

    /// <summary>
    /// The decrease of spread per shot.
    /// </summary>
    [DataField("angleDecay")]
    public Angle AngleDecay = Angle.FromDegrees(4);

    /// <summary>
    /// The max spread angle
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("maxAngle")]
    public Angle MaxAngle = Angle.FromDegrees(2);

    /// <summary>
    /// The min spread angle
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("minAngle")]
    public Angle MinAngle = Angle.FromDegrees(1);



    // Sounds
    /// <summary>
    /// What sound will the turret make when it shoots?
    /// </summary>
    [DataField("soundGunshot")]
    public string SoundGunshot = "/Audio/Weapons/Guns/Gunshots/pistol.ogg";

    /// <summary>
    /// OOPS! All ran out!
    /// </summary>
    [DataField("soundEmpty")]
    public string SoundEmpty = "/Audio/Weapons/Guns/Empty/empty.ogg";


    // Upgrade stuff, I can't be bothered to comment all of this
    [DataField("fireRatePart", customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
    public string PartFiringSpeed = "Manipulator";

    [DataField("fireRatePartMultiplier")]
    public float FireRatePartMultiplier = 0.5f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float FireRateMultiplier = 1;

    [DataField("chargePart", customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
    public string ChargePart = "Capacitor";

    [DataField("chargePartMultiplier")]
    public float ChargePartMultiplier = 0.75f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float ChargeMultiplier = 1;

    [DataField("accuracyPart", customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
    public string AccuracyPart = "Manipulator";

    [DataField("partAccuracyMultiplier")]
    public float AccuracyPartMultiplier = 0.5f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float AccuracyMultiplier = 1;
}
