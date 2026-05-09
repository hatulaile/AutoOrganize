namespace AutoOrganize.Converters.LogsConverter;

public sealed class LogLevelToShortNameConverter : LogEventToStringConverter
{
    public override void SetTextFormatter(string format)
    {
        if (string.IsNullOrEmpty(format))
        {
            base.SetTextFormatter("{Level}");
            return;
        }
        base.SetTextFormatter($"{{Level:{format}}}");
    }

    public LogLevelToShortNameConverter()
        : base("u3")
    {
    }

    public LogLevelToShortNameConverter(string format)
        : base(format)
    {
    }
}