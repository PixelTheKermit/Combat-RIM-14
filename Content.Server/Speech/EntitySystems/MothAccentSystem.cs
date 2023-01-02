using System.Text;
using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems;

public sealed class MothAccentSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<MothAccentComponent, AccentGetEvent>(OnAccent);
    }

    public static string Accentuate(string message)
    {
        var accentedMessage = new StringBuilder(message);

        for (var i = 0; i < accentedMessage.Length; i++)
        {
            var c = accentedMessage[i];

            accentedMessage[i] = c switch
            {
                'a' => 'ä',
                'o' => 'ö',
                'u' => 'ü',
                'A' => 'Ä',
                'O' => 'Ö',
                'U' => 'Ü',
                _ => accentedMessage[i]
            };
        }

        return accentedMessage.ToString();
    }

    private void OnAccent(EntityUid uid, MothAccentComponent component, AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message);
    }
}
