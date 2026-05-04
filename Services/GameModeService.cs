using System.Text;
using System.Windows.Threading;
using WindowsPinboard.Native;

namespace WindowsPinboard.Services;

/// <summary>
/// Detects when a fullscreen app (game / presentation) is active so the sidebar can
/// step out of the way. Combines two signals:
///   1. SHQueryUserNotificationState — official "is fullscreen DirectX/presentation app running"
///   2. Foreground-window-spans-monitor — catches borderless-windowed games that don't
///      flip the QUNS state.
/// </summary>
public sealed class GameModeService : IDisposable
{
    private readonly DispatcherTimer _timer;
    private bool _isFullscreenAppActive;
    private IntPtr _ownHwnd;

    public event Action<bool>? StateChanged;

    public bool IsFullscreenAppActive => _isFullscreenAppActive;

    public GameModeService(int pollIntervalMs = 1500)
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(pollIntervalMs) };
        _timer.Tick += (_, __) => Poll();
    }

    public void SetOwnWindow(IntPtr hwnd) => _ownHwnd = hwnd;

    public void Start()
    {
        Poll(); // immediate first read
        _timer.Start();
    }

    public void Stop() => _timer.Stop();

    private void Poll()
    {
        bool fs = false;

        // Signal 1: shell-reported state.
        if (NativeMethods.SHQueryUserNotificationState(out var state) == 0)
        {
            if (state == NativeMethods.QUNS_RUNNING_D3D_FULL_SCREEN
                || state == NativeMethods.QUNS_PRESENTATION_MODE)
            {
                fs = true;
            }
        }

        // Signal 2: foreground window covers its monitor and isn't us / the shell.
        if (!fs)
        {
            fs = ForegroundWindowIsFullscreen();
        }

        if (fs != _isFullscreenAppActive)
        {
            _isFullscreenAppActive = fs;
            StateChanged?.Invoke(fs);
        }
    }

    private bool ForegroundWindowIsFullscreen()
    {
        var hwnd = NativeMethods.GetForegroundWindow();
        if (hwnd == IntPtr.Zero) return false;
        if (hwnd == _ownHwnd) return false;
        if (hwnd == NativeMethods.GetShellWindow()) return false;
        if (hwnd == NativeMethods.GetDesktopWindow()) return false;

        // Skip well-known shell classes that legitimately span the screen.
        var sb = new StringBuilder(64);
        if (NativeMethods.GetClassName(hwnd, sb, sb.Capacity) > 0)
        {
            var cls = sb.ToString();
            if (cls is "Progman" or "WorkerW" or "Shell_TrayWnd" or "Windows.UI.Core.CoreWindow")
                return false;
        }

        if (!NativeMethods.GetWindowRect(hwnd, out var rect)) return false;

        var hMon = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MONITOR_DEFAULTTONEAREST);
        if (hMon == IntPtr.Zero) return false;

        var info = new NativeMethods.MONITORINFOEX
        {
            cbSize = System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.MONITORINFOEX>()
        };
        if (!NativeMethods.GetMonitorInfo(hMon, ref info)) return false;

        // Match against the FULL monitor bounds (not work area) — fullscreen games cover the whole monitor.
        return rect.Left <= info.rcMonitor.Left
            && rect.Top <= info.rcMonitor.Top
            && rect.Right >= info.rcMonitor.Right
            && rect.Bottom >= info.rcMonitor.Bottom;
    }

    public void Dispose() => _timer.Stop();
}
