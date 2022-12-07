namespace Content.Server.ControllableMob;

[RegisterComponent]
public sealed class ControllableMobComponent : Component
{
    [DataField("range")]
    public float Range = 50f;

    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? CurrentEntityOwning;
}
