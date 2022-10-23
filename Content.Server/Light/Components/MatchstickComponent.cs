using Content.Shared.Smoking;
using Content.Server.Light.EntitySystems;
using Robust.Shared.Audio;
using Content.Shared.Damage;

namespace Content.Server.Light.Components
{
    [RegisterComponent]
    [Access(typeof(MatchstickSystem))]
    public sealed class MatchstickComponent : Component
    {
        /// <summary>
        /// Current state to matchstick. Can be <code>Unlit</code>, <code>Lit</code> or <code>Burnt</code>.
        /// </summary>
        [ViewVariables]
        public SmokableState CurrentState = SmokableState.Unlit;

        /// <summary>
        /// How long will matchstick last in seconds.
        /// </summary>
        [ViewVariables(VVAccess.ReadOnly)]
        [DataField("duration")]
        public int Duration = 10;

        /// <summary>
        /// How much damage will the matchstick do when it's lit
        /// </summary>
        [DataField("litMeleeDamageBonus")]
        public DamageSpecifier LitMeleeDamageBonus = new();

        /// <summary>
        /// Sound played when you ignite the matchstick.
        /// </summary>
        [DataField("igniteSound", required: true)] public SoundSpecifier IgniteSound = default!;
    }
}
