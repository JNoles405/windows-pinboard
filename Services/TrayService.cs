using System.Drawing;
using System.Drawing.Drawing2D;
using Forms = System.Windows.Forms;

namespace WindowsPinboard.Services;

public sealed class TrayService : IDisposable
{
    private readonly Forms.NotifyIcon _icon;
    private readonly Icon _heldIcon;

    public event Action? ShowRequested;
    public event Action? SettingsRequested;
    public event Action? ExitRequested;

    public TrayService()
    {
        _heldIcon = CreateIcon();
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
        menu.Items.Add("Quit", null, (_, __) => ExitRequested?.Invoke());
        _icon.ContextMenuStrip = menu;
    }

    public void ShowBalloon(string title, string message)
    {
        _icon.BalloonTipTitle = title;
        _icon.BalloonTipText = message;
        _icon.ShowBalloonTip(2500);
    }

    private static Icon CreateIcon()
    {
        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            using var bg = new SolidBrush(Color.FromArgb(255, 31, 31, 35));
            g.FillRectangle(bg, 4, 4, 24, 24);
            using var sliver = new SolidBrush(Color.FromArgb(255, 111, 168, 255));
            g.FillRectangle(sliver, 23, 6, 4, 20);
            using var pad = new SolidBrush(Color.FromArgb(255, 60, 60, 70));
            g.FillRectangle(pad, 7, 8, 12, 5);
            g.FillRectangle(pad, 7, 16, 12, 5);
        }
        var hicon = bmp.GetHicon();
        var icon = (Icon)Icon.FromHandle(hicon).Clone();
        Native.NativeMethods.DestroyIcon(hicon);
        return icon;
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
        _heldIcon.Dispose();
    }
}
