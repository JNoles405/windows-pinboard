using System.Drawing;
using WindowsPinboard.Native;
using Forms = System.Windows.Forms;

namespace WindowsPinboard.Services;

public sealed record MonitorInfo(
    string DeviceName,
    string DisplayName,
    Rectangle WorkingAreaPx,
    Rectangle BoundsPx,
    bool IsPrimary,
    double DpiScale)
{
    public double WorkLeftDip => WorkingAreaPx.Left / DpiScale;
    public double WorkTopDip => WorkingAreaPx.Top / DpiScale;
    public double WorkWidthDip => WorkingAreaPx.Width / DpiScale;
    public double WorkHeightDip => WorkingAreaPx.Height / DpiScale;
    public double WorkRightDip => WorkLeftDip + WorkWidthDip;
}

public static class MonitorService
{
    public static IReadOnlyList<MonitorInfo> List()
    {
        var list = new List<MonitorInfo>();
        int idx = 1;
        foreach (var s in Forms.Screen.AllScreens)
        {
            var dpi = GetMonitorDpi(s);
            var label = ShortName(s, idx) + (s.Primary ? " (primary)" : "");
            list.Add(new MonitorInfo(
                DeviceName: s.DeviceName,
                DisplayName: label,
                WorkingAreaPx: s.WorkingArea,
                BoundsPx: s.Bounds,
                IsPrimary: s.Primary,
                DpiScale: dpi));
            idx++;
        }
        return list;
    }

    public static MonitorInfo Resolve(string? preferredDeviceName)
    {
        var all = List();
        if (!string.IsNullOrEmpty(preferredDeviceName))
        {
            var match = all.FirstOrDefault(m => string.Equals(m.DeviceName, preferredDeviceName, StringComparison.OrdinalIgnoreCase));
            if (match is not null) return match;
        }
        return all.FirstOrDefault(m => m.IsPrimary) ?? all.First();
    }

    private static string ShortName(Forms.Screen s, int idx)
    {
        // DeviceName is like \\.\DISPLAY1
        var name = s.DeviceName.Replace("\\\\.\\", "");
        return $"{idx}: {name} ({s.Bounds.Width}×{s.Bounds.Height})";
    }

    private static double GetMonitorDpi(Forms.Screen s)
    {
        // Pick a point inside the screen and query effective DPI for that monitor.
        var pt = new NativeMethods.POINT(s.Bounds.Left + s.Bounds.Width / 2, s.Bounds.Top + s.Bounds.Height / 2);
        var hMon = NativeMethods.MonitorFromPoint(pt, NativeMethods.MONITOR_DEFAULTTONEAREST);
        if (hMon != IntPtr.Zero
            && NativeMethods.GetDpiForMonitor(hMon, NativeMethods.MDT_EFFECTIVE_DPI, out var dpiX, out _) == 0
            && dpiX > 0)
        {
            return dpiX / 96.0;
        }
        return 1.0;
    }
}
