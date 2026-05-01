using System.Windows;
using System.Windows.Interop;
using WindowsPinboard.Native;

namespace WindowsPinboard.Services;

public sealed class HotkeyService : IDisposable
{
    private const int HotkeyId = 0x9001;

    private HwndSource? _source;
    private IntPtr _hwnd;
    private bool _registered;

    public event Action? Pressed;

    public void Attach(Window window)
    {
        var helper = new WindowInteropHelper(window);
        _hwnd = helper.EnsureHandle();
        _source = HwndSource.FromHwnd(_hwnd);
        _source?.AddHook(WndProc);
    }

    public bool Register(uint modifiers, uint vk)
    {
        Unregister();
        var ok = NativeMethods.RegisterHotKey(_hwnd, HotkeyId,
            modifiers | (uint)NativeMethods.HotkeyModifiers.NoRepeat, vk);
        _registered = ok;
        return ok;
    }

    public void Unregister()
    {
        if (_registered)
        {
            NativeMethods.UnregisterHotKey(_hwnd, HotkeyId);
            _registered = false;
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == HotkeyId)
        {
            Pressed?.Invoke();
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        Unregister();
        _source?.RemoveHook(WndProc);
        _source = null;
    }
}
