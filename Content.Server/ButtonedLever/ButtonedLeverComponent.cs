using Content.Shared.MachineLinking;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.MachineLinking.Components
{
    [RegisterComponent]
    public sealed class ButtonedLeverComponent : Component
    {
        [DataField("state")]
        public TwoWayLeverState State;

        [DataField("pressed")]
        public bool Pressed;

        [DataField("nextSignalLeft")]
        public bool NextSignalLeft;

        [DataField("leftPort", customTypeSerializer: typeof(PrototypeIdSerializer<TransmitterPortPrototype>))]
        public string LeftPort = "Left";

        [DataField("rightPort", customTypeSerializer: typeof(PrototypeIdSerializer<TransmitterPortPrototype>))]
        public string RightPort = "Right";

        [DataField("middlePort", customTypeSerializer: typeof(PrototypeIdSerializer<TransmitterPortPrototype>))]
        public string MiddlePort = "Middle";

        [DataField("onPort", customTypeSerializer: typeof(PrototypeIdSerializer<TransmitterPortPrototype>))]
        public string OnPort = "On";

        [DataField("offPort", customTypeSerializer: typeof(PrototypeIdSerializer<TransmitterPortPrototype>))]
        public string OffPort = "Off";

        [DataField("clickSound")]
        public SoundSpecifier ClickSound { get; set; } = new SoundPathSpecifier("/Audio/Machines/lightswitch.ogg");
    }
}
