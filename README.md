# ReminderMe (MAUI)

A .NET MAUI starter app that converts plain-language text into reminders.

## Example input

- `hey myapp, can you please remind me to take meeting at 5 pm today`
- `remind me to send report at 08:30 tomorrow`

The app extracts a time, derives a description, and schedules a local in-app reminder.

## Features

- Plain-language reminder parser (`5 pm`, `8:30 am`, `17:30`).
- Automatic fallback to next day when the entered time has already passed.
- Live reminder list with scheduled/triggered status.
- Pop-up alert when a reminder time is reached.

## Run locally

1. Install the .NET 8 SDK with MAUI workload.
2. Restore workloads:
   ```bash
   dotnet workload install maui
   ```
3. Build and run (example for Android):
   ```bash
   dotnet build -t:Run -f net8.0-android
   ```

## Notes

- This version uses an in-memory list. Add persistent storage (SQLite/Preferences) for production use.
- Native push/local notifications can be added per platform for background reminder delivery.
