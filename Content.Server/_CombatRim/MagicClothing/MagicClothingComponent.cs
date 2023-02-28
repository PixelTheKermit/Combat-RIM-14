using System.Threading;
using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Server._CombatRim.MagicClothing;

/// <summary>
/// Spellbooks for having an entity learn spells as long as they've read the book and it's in their hand.
/// </summary>
[RegisterComponent]
public sealed class MagicClothingComponent : Component
{
    /// <summary>
    /// List of spells that this book has. This is a combination of the WorldSpells, EntitySpells, and InstantSpells.
    /// </summary>
    [ViewVariables]
    public readonly List<ActionType> Spells = new();


    /// <summary>
    /// The slot used to wear the clothing.
    /// </summary>
    [DataField("slot")]
    public readonly string Slot = "shoes";

    /// <summary>
    /// The three fields below is just used for initialization.
    /// </summary>
    [DataField("worldSpells", customTypeSerializer: typeof(PrototypeIdDictionarySerializer<int, WorldTargetActionPrototype>))]
    public readonly Dictionary<string, int> WorldSpells = new();

    [DataField("entitySpells", customTypeSerializer: typeof(PrototypeIdDictionarySerializer<int, EntityTargetActionPrototype>))]
    public readonly Dictionary<string, int> EntitySpells = new();

    [DataField("instantSpells", customTypeSerializer: typeof(PrototypeIdDictionarySerializer<int, InstantActionPrototype>))]
    public readonly Dictionary<string, int> InstantSpells = new();
}
