namespace WindowsPinboard.Models;

public enum SidebarEdge { Right, Left }

public class AppSettings
{
    public double SidebarWidth { get; set; } = 360;
    public double CollapsedWidth { get; set; } = 6;
    public SidebarEdge Edge { get; set; } = SidebarEdge.Right;

    public uint HotkeyVirtualKey { get; set; } = 0x50; // 'P'
    public uint HotkeyModifiers { get; set; } = 0x0001 | 0x0002; // Alt + Ctrl

    public bool HoverToOpen { get; set; } = true;
    public int HoverDelayMs { get; set; } = 220;
    public int AutoHideDelayMs { get; set; } = 600;

    public int AnimationDurationMs { get; set; } = 180;

    /// <summary>Device name like "\\.\DISPLAY1". Null = use primary.</summary>
    public string? MonitorDeviceName { get; set; }

    public bool RegisterAsAppBar { get; set; } = false;
    public bool StartWithWindows { get; set; } = false;

    /// <summary>Hide the sidebar and ignore the hotkey while a fullscreen game / presentation is running.</summary>
    public bool AutoGameMode { get; set; } = true;
}
