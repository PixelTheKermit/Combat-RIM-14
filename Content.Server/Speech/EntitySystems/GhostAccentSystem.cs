using System.Text;
using Content.Server.Speech.Components;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class GhostAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<GhostAccentComponent, AccentGetEvent>(OnAccent);
    }

    public string Accentuate(string message)
    {
        var words = message.Split();
        var accentedMessage = new StringBuilder(message.Length + 2);

        for (var i = 0; i < words.Length; i++)
        {
            var word = words[i];

            if (word.Length > 2)
            {
                foreach (var _ in word)
                {
                    accentedMessage.Append('o');
                }

                if (_random.NextDouble() >= 0.3)
                    accentedMessage.Append('h');
            }
            else
            {
                accentedMessage.Append('o');
                accentedMessage.Append('o');

                if (_random.NextDouble() >= 0.3)
                    accentedMessage.Append('h');
            }
            accentedMessage.Append('!');

            if (i < words.Length - 1)
            accentedMessage.Append(' ');
        }

        return accentedMessage.ToString();
    }

    private void OnAccent(EntityUid uid, GhostAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message);
    }
}
