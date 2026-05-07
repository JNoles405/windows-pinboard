using System.Drawing;
using Forms = System.Windows.Forms;
using Wpf = System.Windows;

namespace WindowsPinboard.Services;

public sealed class TrayService : IDisposable
{
    private readonly Forms.NotifyIcon _icon;
    private readonly Icon _heldIcon;
    private readonly Forms.ToolStripMenuItem _gameModeItem;

    public event Action? ShowRequested;
    public event Action? SettingsRequested;
    public event Action? ExitRequested;

    public TrayService()
    {
        _heldIcon = LoadAppIcon();
        _icon = new Forms.NotifyIcon
        {
            Icon = _heldIcon,
            Visible = true,
            Text = "Windows Pinboard",
        };
        _icon.MouseClick += (_, e) =>
        {
            if (e.Button == Forms.MouseButtons.Left) ShowRequested?.Invoke();
        };

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Show sidebar", null, (_, __) => ShowRequested?.Invoke());
        menu.Items.Add("Settings…", null, (_, __) => SettingsRequested?.Invoke());
        menu.Items.Add(new Forms.ToolStripSeparator());

        _gameModeItem = new Forms.ToolStripMenuItem("Game mode: off")
        {
            Enabled = false, // status indicator only
        };
        menu.Items.Add(_gameModeItem);

        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Quit", null, (_, __) => ExitRequested?.Invoke());
        _icon.ContextMenuStrip = menu;
    }

    public void SetGameModeIndicator(bool active)
    {
        _gameModeItem.Text = active ? "Game mode: ACTIVE (paused)" : "Game mode: off";
        _icon.Text = active ? "Windows Pinboard — paused (game mode)" : "Windows Pinboard";
    }

    public void ShowBalloon(string title, string message)
    {
        _icon.BalloonTipTitle = title;
        _icon.BalloonTipText = message;
        _icon.ShowBalloonTip(2500);
    }

    /// <summary>
    /// Loads the multi-resolution app icon from the embedded WPF resource so the tray
    /// icon, the window icon, and the .exe icon all share one source.
    /// </summary>
    private static Icon LoadAppIcon()
    {
        var uri = new Uri("pack://application:,,,/Assets/app.ico", UriKind.Absolute);
        var sri = Wpf.Application.GetResourceStream(uri)
            ?? throw new InvalidOperationException("Embedded app.ico resource not found.");
        using var stream = sri.Stream;

        // Pick the frame that best matches the system small icon size (typically 16x16).
        var size = Forms.SystemInformation.SmallIconSize;
        return new Icon(stream, size.Width, size.Height);
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
        _heldIcon.Dispose();
    }
}
