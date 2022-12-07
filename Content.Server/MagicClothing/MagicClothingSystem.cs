using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Inventory.Events;
using Robust.Shared.Prototypes;

namespace Content.Server.MagicClothing;

public sealed class MagicClothingSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MagicClothingComponent, GotEquippedEvent>(OnGotEquipped);
    }

    private void OnGotEquipped(EntityUid uid, MagicClothingComponent component, GotEquippedEvent args)
    {
        if (args.Slot != component.Slot)
            return;

        //Negative charges means the spell can be used without it running out.
        foreach (var (id, charges) in component.WorldSpells)
        {
            var spell = new WorldTargetAction(_prototypeManager.Index<WorldTargetActionPrototype>(id));
            _actionsSystem.SetCharges(spell, charges < 0 ? null : charges);
            component.Spells.Add(spell);
        }

        foreach (var (id, charges) in component.InstantSpells)
        {
            var spell = new InstantAction(_prototypeManager.Index<InstantActionPrototype>(id));
            _actionsSystem.SetCharges(spell, charges < 0 ? null : charges);
            component.Spells.Add(spell);
        }

        foreach (var (id, charges) in component.EntitySpells)
        {
            var spell = new EntityTargetAction(_prototypeManager.Index<EntityTargetActionPrototype>(id));
            _actionsSystem.SetCharges(spell, charges < 0 ? null : charges);
            component.Spells.Add(spell);
        }

        Logger.Warning("work damn it");
        _actionsSystem.AddActions(args.Equipee, component.Spells, uid);
    }
}
