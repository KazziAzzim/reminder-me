using System.Globalization;
using CommunityToolkit.Maui.Media;

namespace ReminderMe.Services;

public sealed class VoiceInputService : IVoiceInputService
{
    public async Task<VoiceInputResult> ListenAsync(CancellationToken cancellationToken)
    {
        try
        {
            var permission = await SpeechToText.Default.RequestPermissions(cancellationToken);
            if (!permission)
            {
                return VoiceInputResult.Failed("Microphone/Speech permission is required.");
            }

            var result = await SpeechToText.Default.ListenAsync(
                CultureInfo.CurrentCulture,
                progress: null,
                cancellationToken);

            if (!result.IsSuccessful || string.IsNullOrWhiteSpace(result.Text))
            {
                return VoiceInputResult.Failed("I couldn't understand the voice command. Please try again.");
            }

            return VoiceInputResult.Ok(result.Text.Trim());
        }
        catch (Exception ex)
        {
            return VoiceInputResult.Failed($"Voice input failed: {ex.Message}");
        }
    }
}
