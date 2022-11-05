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

    // Sounds

    [DataField("soundGunshot")]
    public string SoundGunshot = "/Audio/Weapons/Guns/Gunshots/pistol.ogg";

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
}
