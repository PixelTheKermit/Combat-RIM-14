using Content.Server.Light.EntitySystems;
using Content.Shared.Smoking;
using Robust.Shared.Audio;
using Content.Shared.Damage;
using System.Threading;

namespace Content.Server.Light.Components
{
    [RegisterComponent]
    [Access(typeof(MatchstickSystem))]
    public sealed class MatchstickComponent : Component
    {
        /// <summary>
        /// Current state to matchstick. Can be <code>Unlit</code>, <code>Lit</code> or <code>Burnt</code>.
        /// </summary>
        [DataField("state")]
        public SmokableState CurrentState = SmokableState.Unlit;

        /// <summary>
        /// How long will matchstick last in seconds.
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        [DataField("duration")]
        public int Duration = 10;

        [ViewVariables(VVAccess.ReadOnly)]
        [DataField("delay")]
        public int Delay = 1;

        /// <summary>
        /// Sound played when you ignite the matchstick.
        /// </summary>
        [DataField("igniteSound", required: true)] public SoundSpecifier IgniteSound = default!;
    }
}
