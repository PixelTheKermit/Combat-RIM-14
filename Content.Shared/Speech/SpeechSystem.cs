namespace Content.Shared.Speech
{
    public sealed class SpeechSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SpeakAttemptEvent>(OnSpeakAttempt);
            SubscribeLocalEvent<WhisperAttemptEvent>(OnWhisperAttempt);
        }

        private void OnSpeakAttempt(SpeakAttemptEvent args)
        {
            if (!TryComp(args.Uid, out SharedSpeechComponent? speech) || !speech.Enabled)
                args.Cancel();
        }
        private void OnWhisperAttempt(WhisperAttemptEvent args)
        {
            if (!TryComp(args.Uid, out SharedSpeechComponent? whisper) || !whisper.Enabled)
                args.Cancel();
        }
    }
}
