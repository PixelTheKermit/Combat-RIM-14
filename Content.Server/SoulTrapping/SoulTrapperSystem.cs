using Content.Shared.Damage;
using Content.Shared.Interaction;

namespace Content.Server.SoulTrapping;

public sealed class SoulTrapperSystem : EntitySystem
{
    // Dependencies
    [Dependency] private readonly EntityManager _entityManager = default!;
    public override void Initialize() // VERY IMPORTANT!!!!!!
    {
        base.Initialize();
    }
}
