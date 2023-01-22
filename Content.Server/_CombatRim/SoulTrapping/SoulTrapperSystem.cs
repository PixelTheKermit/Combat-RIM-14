using Content.Server.Mind.Components;
using Content.Shared._CombatRim.SoulTrapping.Components;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server._CombatRim.SoulTrapping;

public sealed class SoulTrapperSystem : EntitySystem
{
    // Dependencies
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    public override void Initialize() // VERY IMPORTANT!!!!!!
    {
        base.Initialize();

        SubscribeLocalEvent<SoulTrapperComponent, EntInsertedIntoContainerMessage>(OnInsert);
        SubscribeLocalEvent<SoulTrapperComponent, EntRemovedFromContainerMessage>(OnEject);
    }


    private void OnInsert(EntityUid uid, SoulTrapperComponent comp, EntInsertedIntoContainerMessage args)
    {
        _appearanceSystem.SetData(uid, SoulTrapperVisuals.Inserted, 1);

        if (Comp<MindComponent>(args.Entity).Mind != null)
        {
            _appearanceSystem.SetData(uid, SoulTrapperVisuals.Inserted, 2);
        }

        Dirty(uid);
    }

    private void OnEject(EntityUid uid, SoulTrapperComponent comp, EntRemovedFromContainerMessage args)
    {
        _appearanceSystem.SetData(uid, SoulTrapperVisuals.Inserted, 0);
        Dirty(uid);
    }
}
