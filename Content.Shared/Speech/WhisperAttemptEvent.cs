namespace Content.Shared.Speech
{
    public sealed class WhisperAttemptEvent : CancellableEntityEventArgs
    {
        public WhisperAttemptEvent(EntityUid uid)
        {
            Uid = uid;
        }

        public EntityUid Uid { get; }
    }
}
