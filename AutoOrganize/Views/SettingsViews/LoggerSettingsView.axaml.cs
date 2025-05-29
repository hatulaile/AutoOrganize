using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Serilog.Events;
using Ursa.Controls;
using NumericUpDown = Ursa.Controls.NumericUpDown;

namespace AutoOrganize.Views.SettingsViews;

public partial class LoggerSettingsView : UserControl
{
    public static readonly IReadOnlyList<LogEventLevel> LogEventLevels = Enum.GetValues<LogEventLevel>();

    public LoggerSettingsView()
    {
        InitializeComponent();
    }
}