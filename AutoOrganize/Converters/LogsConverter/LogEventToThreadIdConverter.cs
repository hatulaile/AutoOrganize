namespace AutoOrganize.Converters.LogsConverter;

public sealed class LogEventToThreadIdConverter : LogEventToStringConverter
{
    public override void SetTextFormatter(string format)
    {
        if (string.IsNullOrEmpty(format))
        {
            base.SetTextFormatter("{ThreadId}");
            return;
        }
        base.SetTextFormatter(format);
    }

    public LogEventToThreadIdConverter() : base(string.Empty)
    {

    }

    public LogEventToThreadIdConverter(string format) : base($"{{LogEventToThreadIdConverter:{format}}}")
    {
    }
}