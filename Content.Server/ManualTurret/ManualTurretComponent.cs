using Content.Shared.Construction.Prototypes;
using Content.Shared.MachineLinking;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Timing;

namespace Content.Server.ManualTurret;

[RegisterComponent]
public sealed class ManualTurretComponent : Component
{

    // Stuff for le turret

    [DataField("fireRate")]
    public float FireRate = 1f;

    public TimeSpan TimeFired = TimeSpan.Zero;

    public bool Firing = false;

    [DataField("rotSpeed")]
    public float RotSpeed = 1f; // This is divided by 100, so rotation is slower than this

    [DataField("curRotSpeed")]
    public float CurRotSpeed = 0f;

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

    // SIGNALS PAST THIS POINT

    [DataField("clockwiseRot", customTypeSerializer: typeof(PrototypeIdSerializer<ReceiverPortPrototype>))]
    public string ClockwiseRot = "TurretClockwise";

    [DataField("antiClockwiseRot", customTypeSerializer: typeof(PrototypeIdSerializer<ReceiverPortPrototype>))]
    public string AntiClockwiseRot = "TurretAntiClockwise";

    [DataField("stopRot", customTypeSerializer: typeof(PrototypeIdSerializer<ReceiverPortPrototype>))]
    public string StopRot = "TurretStopRot";

    [DataField("turretShootSemi", customTypeSerializer: typeof(PrototypeIdSerializer<ReceiverPortPrototype>))]
    public string TurretSemi = "TurretShootSemi";

    [DataField("turretShootAutoToggle", customTypeSerializer: typeof(PrototypeIdSerializer<ReceiverPortPrototype>))]
    public string TurretAutoToggle = "TurretShootAutoToggle";

    [DataField("turretShootAutoOn", customTypeSerializer: typeof(PrototypeIdSerializer<ReceiverPortPrototype>))]
    public string TurretAutoOn = "TurretShootAutoOn";

    [DataField("turretShootAutoOff", customTypeSerializer: typeof(PrototypeIdSerializer<ReceiverPortPrototype>))]
    public string TurretAutoOff = "TurretShootAutoOff";

    // Part stuff past this point
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
    public float PartRatingAccuracyMultiplier = 0.25f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float AccuracyMultiplier = 1;
}
