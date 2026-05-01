using System.Text;
using System.Windows.Input;
using WindowsPinboard.Native;

namespace WindowsPinboard.Services;

public static class HotkeyFormatter
{
    public static string Format(uint modifiers, uint vk)
    {
        var sb = new StringBuilder();
        var m = (NativeMethods.HotkeyModifiers)modifiers;
        if (m.HasFlag(NativeMethods.HotkeyModifiers.Control)) sb.Append("Ctrl + ");
        if (m.HasFlag(NativeMethods.HotkeyModifiers.Alt)) sb.Append("Alt + ");
        if (m.HasFlag(NativeMethods.HotkeyModifiers.Shift)) sb.Append("Shift + ");
        if (m.HasFlag(NativeMethods.HotkeyModifiers.Win)) sb.Append("Win + ");
        sb.Append(KeyName(vk));
        return sb.ToString();
    }

    public static string KeyName(uint vk)
    {
        try
        {
            var key = KeyInterop.KeyFromVirtualKey((int)vk);
            return key == Key.None ? $"VK{vk:X2}" : key.ToString();
        }
        catch { return $"VK{vk:X2}"; }
    }

    public static (uint modifiers, uint vk)? FromKeyEvent(Key key, ModifierKeys mods)
    {
        if (key == Key.LeftCtrl || key == Key.RightCtrl ||
            key == Key.LeftAlt || key == Key.RightAlt ||
            key == Key.LeftShift || key == Key.RightShift ||
            key == Key.LWin || key == Key.RWin ||
            key == Key.System || key == Key.None)
            return null;

        uint m = 0;
        if (mods.HasFlag(ModifierKeys.Control)) m |= (uint)NativeMethods.HotkeyModifiers.Control;
        if (mods.HasFlag(ModifierKeys.Alt)) m |= (uint)NativeMethods.HotkeyModifiers.Alt;
        if (mods.HasFlag(ModifierKeys.Shift)) m |= (uint)NativeMethods.HotkeyModifiers.Shift;
        if (mods.HasFlag(ModifierKeys.Windows)) m |= (uint)NativeMethods.HotkeyModifiers.Win;
        if (m == 0) return null;

        var actualKey = key == Key.System ? Key.None : key;
        var vk = (uint)KeyInterop.VirtualKeyFromKey(actualKey);
        if (vk == 0) return null;
        return (m, vk);
    }
}
