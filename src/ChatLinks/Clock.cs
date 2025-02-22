namespace SL.ChatLinks;

internal sealed class Clock : IDisposable
{
    public event EventHandler? MinuteStarted;

    public event EventHandler? HourStarted;

    private readonly Timer _minuteEnded;

    private readonly Timer _hourEnded;

    public Clock()
    {
        DateTime now = DateTime.Now;
        DateTime nextMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Local)
            .AddMinutes(1);
        DateTime nextHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Local)
            .AddHours(1);
        TimeSpan minuteDue = nextMinute - now;
        TimeSpan hourDue = nextHour - now;

        _minuteEnded = new(OnMinuteStart, null, minuteDue, TimeSpan.FromMinutes(1));
        _hourEnded = new(OnHourStart, null, hourDue, TimeSpan.FromHours(1));
    }

    private void OnMinuteStart(object state)
    {
        MinuteStarted?.Invoke(this, EventArgs.Empty);
    }

    private void OnHourStart(object state)
    {
        HourStarted?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        MinuteStarted = null;
        HourStarted = null;
        _minuteEnded.Dispose();
        _hourEnded.Dispose();
    }
}
