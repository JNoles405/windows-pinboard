using System.Windows;
using System.Windows.Interop;
using WindowsPinboard.Models;
using WindowsPinboard.Native;

namespace WindowsPinboard.Services;

/// <summary>
/// Registers the window as a Windows AppBar so apps reserve screen-edge space for it.
/// We register only the *collapsed sliver* width, so when the sidebar expands it floats
/// above other apps (topmost) without changing the reserved area.
/// </summary>
public sealed class AppBarService : IDisposable
{
    private uint _callbackMessage;
    private bool _registered;
    private IntPtr _hwnd;

    public bool IsRegistered => _registered;
    public uint CallbackMessage => _callbackMessage;

    public event Action? PositionChanged;

    public void Register(Window window)
    {
        if (_registered) return;
        var helper = new WindowInteropHelper(window);
        _hwnd = helper.EnsureHandle();

        var src = HwndSource.FromHwnd(_hwnd);
        src?.AddHook(WndProc);

        _callbackMessage = RegisterWindowMessage("AppBarMessage_WindowsPinboard_" + Guid.NewGuid().ToString("N"));

        var data = new NativeMethods.APPBARDATA
        {
            cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.APPBARDATA>(),
            hWnd = _hwnd,
            uCallbackMessage = _callbackMessage,
        };
        NativeMethods.SHAppBarMessage(NativeMethods.ABM_NEW, ref data);
        _registered = true;
    }

    public void SetPosition(SidebarEdge edge, MonitorInfo monitor, int sliverWidthPx)
    {
        if (!_registered) return;

        var screen = monitor.BoundsPx;
        var data = new NativeMethods.APPBARDATA
        {
            cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.APPBARDATA>(),
            hWnd = _hwnd,
            uEdge = edge == SidebarEdge.Right ? (uint)NativeMethods.ABE_RIGHT : (uint)NativeMethods.ABE_LEFT,
        };

        if (edge == SidebarEdge.Right)
        {
            data.rc = new NativeMethods.RECT(screen.Right - sliverWidthPx, screen.Top, screen.Right, screen.Bottom);
        }
        else
        {
            data.rc = new NativeMethods.RECT(screen.Left, screen.Top, screen.Left + sliverWidthPx, screen.Bottom);
        }

        // Ask shell where it would actually allow us to be, then set.
        NativeMethods.SHAppBarMessage(NativeMethods.ABM_QUERYPOS, ref data);

        // Re-clamp so we keep our intended thickness (QUERYPOS may shrink only).
        if (edge == SidebarEdge.Right)
        {
            data.rc.Left = data.rc.Right - sliverWidthPx;
        }
        else
        {
            data.rc.Right = data.rc.Left + sliverWidthPx;
        }

        NativeMethods.SHAppBarMessage(NativeMethods.ABM_SETPOS, ref data);
    }

    public void Unregister()
    {
        if (!_registered) return;
        var data = new NativeMethods.APPBARDATA
        {
            cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.APPBARDATA>(),
            hWnd = _hwnd,
        };
        NativeMethods.SHAppBarMessage(NativeMethods.ABM_REMOVE, ref data);
        _registered = false;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (_callbackMessage != 0 && (uint)msg == _callbackMessage)
        {
            if (wParam.ToInt32() == NativeMethods.ABN_POSCHANGED)
            {
                PositionChanged?.Invoke();
            }
            handled = true;
        }
        return IntPtr.Zero;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern uint RegisterWindowMessage(string lpString);

    public void Dispose() => Unregister();
}
