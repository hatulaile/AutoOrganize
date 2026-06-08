using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Serilog.Events;

namespace AutoOrganize.Views.SettingsViews;

public partial class LoggerSettingsView : UserControl
{
    public static readonly IReadOnlyList<LogEventLevel> LogEventLevels = Enum.GetValues<LogEventLevel>();

    public LoggerSettingsView()
    {
        InitializeComponent();
    }
}