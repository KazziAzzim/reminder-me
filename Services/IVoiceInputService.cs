namespace ReminderMe.Services;

public interface IVoiceInputService
{
    Task<VoiceInputResult> ListenAsync(CancellationToken cancellationToken);
}

public sealed record VoiceInputResult(bool Success, string Text, string Error)
{
    public static VoiceInputResult Failed(string error) => new(false, string.Empty, error);
    public static VoiceInputResult Ok(string text) => new(true, text, string.Empty);
}
