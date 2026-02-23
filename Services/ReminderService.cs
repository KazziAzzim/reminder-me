using ReminderMe.Models;

namespace ReminderMe.Services;

public sealed class ReminderService : IDisposable
{
    private readonly IDispatcherTimer _timer;
    private readonly List<ReminderItem> _reminders = [];

    public event EventHandler<ReminderItem>? ReminderTriggered;

    public ReminderService(IDispatcher dispatcher)
    {
        _timer = dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(30);
        _timer.Tick += (_, _) => CheckReminders();
        _timer.Start();
    }

    public IReadOnlyList<ReminderItem> GetAll() => _reminders
        .OrderBy(item => item.DueAt)
        .ToList();

    public void Add(ReminderItem reminder)
    {
        _reminders.Add(reminder);
    }

    private void CheckReminders()
    {
        var now = DateTime.Now;
        foreach (var reminder in _reminders.Where(item => !item.IsTriggered && item.DueAt <= now))
        {
            reminder.IsTriggered = true;
            ReminderTriggered?.Invoke(this, reminder);
        }
    }

    public void Dispose()
    {
        _timer.Stop();
    }
}
