using ReminderMe.Services;

namespace ReminderMe.Tests;

public class ReminderParserTests
{
    private readonly ReminderParser _parser = new();

    [Fact]
    public void TryParse_ParsesWakePhraseCommand()
    {
        var now = new DateTime(2026, 1, 10, 9, 0, 0);

        var ok = _parser.TryParse(
            "hey myapp add reminder to take meeting at 5 pm today",
            now,
            out var reminder,
            out var error);

        Assert.True(ok);
        Assert.NotNull(reminder);
        Assert.Equal(string.Empty, error);
        Assert.Equal("Take Meeting", reminder!.Description);
        Assert.Equal(new DateTime(2026, 1, 10, 17, 0, 0), reminder.DueAt);
    }

    [Fact]
    public void TryParse_SchedulesForTomorrowWhenTimeHasPassed()
    {
        var now = new DateTime(2026, 1, 10, 20, 0, 0);

        var ok = _parser.TryParse(
            "remind me to send report at 5 pm today",
            now,
            out var reminder,
            out _);

        Assert.True(ok);
        Assert.NotNull(reminder);
        Assert.Equal(new DateTime(2026, 1, 11, 17, 0, 0), reminder!.DueAt);
    }
}
