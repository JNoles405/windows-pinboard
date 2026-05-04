using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WindowsPinboard.Models;
using WindowsPinboard.Services;

namespace WindowsPinboard.Views;

public partial class SettingsWindow : Window
{
    private uint _hotkeyMods;
    private uint _hotkeyVk;

    public AppSettings Result { get; private set; }

    public SettingsWindow(AppSettings current)
    {
        InitializeComponent();
        Result = Clone(current);

        _hotkeyMods = current.HotkeyModifiers;
        _hotkeyVk = current.HotkeyVirtualKey;
        UpdateHotkeyDisplay();

        WidthBox.Text = current.SidebarWidth.ToString(CultureInfo.InvariantCulture);
        HandleBox.Text = current.CollapsedWidth.ToString(CultureInfo.InvariantCulture);
        HoverCheck.IsChecked = current.HoverToOpen;
        HoverDelayBox.Text = current.HoverDelayMs.ToString(CultureInfo.InvariantCulture);
        AutoHideBox.Text = current.AutoHideDelayMs.ToString(CultureInfo.InvariantCulture);
        AppBarCheck.IsChecked = current.RegisterAsAppBar;
        AutostartCheck.IsChecked = current.StartWithWindows;
        GameModeCheck.IsChecked = current.AutoGameMode;

        foreach (ComboBoxItem item in EdgeCombo.Items)
        {
            if ((string)item.Tag == current.Edge.ToString())
            {
                EdgeCombo.SelectedItem = item;
                break;
            }
        }

        var monitors = MonitorService.List();
        MonitorCombo.ItemsSource = monitors;
        var selected = monitors.FirstOrDefault(m =>
            string.Equals(m.DeviceName, current.MonitorDeviceName, StringComparison.OrdinalIgnoreCase))
            ?? monitors.FirstOrDefault(m => m.IsPrimary)
            ?? monitors.FirstOrDefault();
        MonitorCombo.SelectedItem = selected;
    }

    private static AppSettings Clone(AppSettings s) => new()
    {
        SidebarWidth = s.SidebarWidth,
        CollapsedWidth = s.CollapsedWidth,
        Edge = s.Edge,
        HotkeyModifiers = s.HotkeyModifiers,
        HotkeyVirtualKey = s.HotkeyVirtualKey,
        HoverToOpen = s.HoverToOpen,
        HoverDelayMs = s.HoverDelayMs,
        AutoHideDelayMs = s.AutoHideDelayMs,
        AnimationDurationMs = s.AnimationDurationMs,
        MonitorDeviceName = s.MonitorDeviceName,
        RegisterAsAppBar = s.RegisterAsAppBar,
        StartWithWindows = s.StartWithWindows,
        AutoGameMode = s.AutoGameMode,
    };

    private void UpdateHotkeyDisplay()
    {
        HotkeyBox.Text = HotkeyFormatter.Format(_hotkeyMods, _hotkeyVk);
    }

    private void HotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        var combo = HotkeyFormatter.FromKeyEvent(key, Keyboard.Modifiers);
        if (combo is { } c)
        {
            _hotkeyMods = c.modifiers;
            _hotkeyVk = c.vk;
            UpdateHotkeyDisplay();
        }
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        if (!double.TryParse(WidthBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var width) || width < 120 || width > 1200)
        {
            MessageBox.Show("Sidebar width must be between 120 and 1200 px.", "Invalid", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!double.TryParse(HandleBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var handle) || handle < 2 || handle > 24)
        {
            MessageBox.Show("Handle thickness must be between 2 and 24 px.", "Invalid", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!int.TryParse(HoverDelayBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var hover) || hover < 0 || hover > 5000) hover = 220;
        if (!int.TryParse(AutoHideBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var autoHide) || autoHide < 0 || autoHide > 10000) autoHide = 600;

        Result.SidebarWidth = width;
        Result.CollapsedWidth = handle;
        Result.HotkeyModifiers = _hotkeyMods;
        Result.HotkeyVirtualKey = _hotkeyVk;
        Result.HoverToOpen = HoverCheck.IsChecked == true;
        Result.HoverDelayMs = hover;
        Result.AutoHideDelayMs = autoHide;
        Result.RegisterAsAppBar = AppBarCheck.IsChecked == true;
        Result.StartWithWindows = AutostartCheck.IsChecked == true;
        Result.AutoGameMode = GameModeCheck.IsChecked == true;

        if (EdgeCombo.SelectedItem is ComboBoxItem ci && (string)ci.Tag is string tag
            && Enum.TryParse<SidebarEdge>(tag, out var edge))
        {
            Result.Edge = edge;
        }

        if (MonitorCombo.SelectedItem is MonitorInfo mi)
        {
            Result.MonitorDeviceName = mi.DeviceName;
        }

        DialogResult = true;
        Close();
    }
}
