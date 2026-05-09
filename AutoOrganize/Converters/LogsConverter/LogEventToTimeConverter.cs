namespace AutoOrganize.Converters.LogsConverter;

public sealed class LogEventToTimeConverter: LogEventToStringConverter
{
    public override void SetTextFormatter(string format)
    {
        if (string.IsNullOrEmpty(format))
        {
            base.SetTextFormatter("{Timestamp}");
            return;
        }
        base.SetTextFormatter($"{{Timestamp:{format}}}");
    }

    public LogEventToTimeConverter() : base("yyyy-MM-dd HH:mm:ss")
    {
    }

    public LogEventToTimeConverter(string format) : base(format)
    {
    }
}