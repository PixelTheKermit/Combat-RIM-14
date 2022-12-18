namespace Content.Shared.Chat;

public static class ChatChannelExtensions
{
    public static Color TextColor(this ChatChannel channel)
    {
        return channel switch
        {
            ChatChannel.Server => Color.Orange,
            ChatChannel.Radio => Color.FromHex("#32861A"),
            ChatChannel.LOOC => Color.FromHex("#429E9B"),
            ChatChannel.OOC => Color.FromHex("#709FBD"),
            ChatChannel.Dead => Color.FromHex("#715AA1"),
            ChatChannel.Admin => Color.FromHex("#B30808"),
            ChatChannel.Whisper => Color.DarkGray,
            _ => Color.LightGray
        };
    }
}
