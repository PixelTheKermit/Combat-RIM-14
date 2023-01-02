using Content.Shared.Damage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Server._CombatRim.Cauterize;

[RegisterComponent]
public sealed class CauterizeComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("delay")]
    public int Delay = 1;

    /// <summary>
    /// How much damage will the matchstick do when it's lit
    /// </summary>
    [DataField("litCauterizeDamage")]
    public DamageSpecifier LitCauterizeDamage = new();

    public CancellationTokenSource? CancelToken;
}
