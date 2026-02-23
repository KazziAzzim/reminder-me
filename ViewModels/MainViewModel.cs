using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ReminderMe.Models;
using ReminderMe.Services;

namespace ReminderMe.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private const string WakePhrase = "hey myapp add reminder";

    private readonly ReminderParser _parser;
    private readonly ReminderService _service;
    private readonly IVoiceInputService _voiceInputService;

    private string _inputText = string.Empty;
    private string _statusMessage = "Ready to create reminders.";
    private bool _isListening;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ReminderDisplayItem> Reminders { get; } = [];

    public ICommand CreateReminderCommand { get; }
    public ICommand StartVoiceCommand { get; }

    public string InputText
    {
        get => _inputText;
        set => SetProperty(ref _inputText, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsListening
    {
        get => _isListening;
        set => SetProperty(ref _isListening, value);
    }

    public MainViewModel(ReminderParser parser, ReminderService service, IVoiceInputService voiceInputService)
    {
        _parser = parser;
        _service = service;
        _voiceInputService = voiceInputService;
        _service.ReminderTriggered += OnReminderTriggered;

        CreateReminderCommand = new Command(CreateReminder);
        StartVoiceCommand = new Command(async () => await ListenAndCreateReminderAsync());
        RefreshReminders();
    }

    private async Task ListenAndCreateReminderAsync()
    {
        if (IsListening)
        {
            return;
        }

        IsListening = true;
        StatusMessage = "Listening for voice command...";

        var result = await _voiceInputService.ListenAsync(CancellationToken.None);
        IsListening = false;

        if (!result.Success)
        {
            StatusMessage = result.Error;
            return;
        }

        InputText = result.Text;
        if (InputText.Contains(WakePhrase, StringComparison.OrdinalIgnoreCase))
        {
            CreateReminder();
        }
        else
        {
            StatusMessage = "Voice captured. Say 'Hey myapp add reminder ...' to auto-create.";
        }
    }

    private async void OnReminderTriggered(object? sender, ReminderItem reminder)
    {
        RefreshReminders();
        StatusMessage = $"⏰ Reminder: {reminder.Description}";
        if (Application.Current?.MainPage is not null)
        {
            await Application.Current.MainPage.DisplayAlert("Reminder", reminder.Description, "OK");
        }
    }

    private void CreateReminder()
    {
        if (!_parser.TryParse(InputText, DateTime.Now, out var reminder, out var error) || reminder is null)
        {
            StatusMessage = error;
            return;
        }

        _service.Add(reminder);
        StatusMessage = $"Reminder created for {reminder.DueAt:g}";
        InputText = string.Empty;
        RefreshReminders();
    }

    private void RefreshReminders()
    {
        Reminders.Clear();
        foreach (var reminder in _service.GetAll())
        {
            Reminders.Add(ReminderDisplayItem.From(reminder));
        }
    }

    private void SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingField, value))
        {
            return;
        }

        backingField = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed class ReminderDisplayItem
{
    public required string Description { get; init; }
    public required string DueAtText { get; init; }
    public required string StatusText { get; init; }

    public static ReminderDisplayItem From(ReminderItem item) => new()
    {
        Description = item.Description,
        DueAtText = item.DueAt.ToString("f"),
        StatusText = item.IsTriggered ? "Triggered" : "Scheduled"
    };
}
