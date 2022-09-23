using Content.Shared.CharacterAppearance;
using Content.Shared.Markings;
using Content.Shared.Species;
using Content.Shared.Humanoid.Markings;

namespace Content.Shared.IoC
{
    public static class SharedContentIoC
    {
        public static void Register()
        {
            IoCManager.Register<MarkingManager, MarkingManager>();
            IoCManager.Register<SpeciesManager>();
        }
    }
}
