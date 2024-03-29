using Content.Shared.Inventory;

namespace Content.Shared.Radio.Components;

/// <summary>
///     This component relays radio messages to the parent entity's chat when equipped.
/// </summary>
[RegisterComponent]
public sealed class HeadsetComponent : Component
{
    [DataField("enabled")]
    public bool Enabled = true;

    public bool IsEquipped = false;

    [DataField("requiredSlot")]
    public SlotFlags RequiredSlot = SlotFlags.EARS;

    [DataField("range")]
    public float Range = 50f;
}
