using Content.Shared.Construction.Prototypes;
using Content.Shared.MachineLinking;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Timing;

namespace Content.Server._CombatRim.ManualTurret;

[RegisterComponent]
public sealed class ManualTurretComponent : Component
{

    // Stuff for le turret

    [DataField("fullAuto")]
    public bool FullAuto = false;

    [DataField("fireRate")]
    public float FireRate = 1.5f;

    public TimeSpan TimeFired = TimeSpan.Zero;

    public bool Firing = false;

    [DataField("rotSpeed")]
    public float RotSpeed = 2f; // This is divided by 100, so rotation is slower than this

    [DataField("currentRot")]
    public Angle Rotation;

    [DataField("isBatteryWeapon")]
    public bool IsBatteryWeapon = false;

    [DataField("fireCost")]
    public float FireCost = 0f;

    [DataField("currentAngle")]
    public Angle CurrentAngle;

    [ViewVariables(VVAccess.ReadWrite), DataField("angleIncrease")]
    public Angle AngleIncrease = Angle.FromDegrees(0.5);

    [DataField("angleDecay")]
    public Angle AngleDecay = Angle.FromDegrees(4);

    [ViewVariables(VVAccess.ReadWrite), DataField("maxAngle")]
    public Angle MaxAngle = Angle.FromDegrees(2);

    [ViewVariables(VVAccess.ReadWrite), DataField("minAngle")]
    public Angle MinAngle = Angle.FromDegrees(1);



    // Sounds

    [DataField("soundGunshot")]
    public string SoundGunshot = "/Audio/Weapons/Guns/Gunshots/pistol.ogg";

    [DataField("soundEmpty")]
    public string SoundEmpty = "/Audio/Weapons/Guns/Empty/empty.ogg";


    // Upgrade stuff.
    [DataField("machinePartFiringSpeed", customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
    public string MachinePartFiringSpeed = "Manipulator";

    [DataField("partRatingFiringSpeedMultiplier")]
    public float PartRatingFireRateMultiplier = 0.5f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float FireRateMultiplier = 1;

    [DataField("machinePartChargeNeeded", customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
    public string MachinePartChargeNeeded = "Capacitor";

    [DataField("partRatingChargeNeededMultiplier")]
    public float PartRatingChargeNeededMultiplier = 0.75f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float ChargeNeededMultiplier = 1;

    [DataField("machinePartAccuracy", customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
    public string MachinePartAccuracy = "Laser";

    [DataField("partRatingAccuracyMultiplier")]
    public float PartRatingAccuracyMultiplier = 0.5f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float AccuracyMultiplier = 1;
}
