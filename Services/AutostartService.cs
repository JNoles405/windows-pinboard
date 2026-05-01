using Microsoft.Win32;

namespace WindowsPinboard.Services;

public static class AutostartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "WindowsPinboard";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        var value = key?.GetValue(ValueName) as string;
        if (string.IsNullOrEmpty(value)) return false;
        var current = ExecutablePathForRegistry();
        return string.Equals(value, current, StringComparison.OrdinalIgnoreCase);
    }

    public static void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true)
                        ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
        if (enabled)
        {
            key!.SetValue(ValueName, ExecutablePathForRegistry(), RegistryValueKind.String);
        }
        else
        {
            if (key?.GetValue(ValueName) is not null)
                key.DeleteValue(ValueName, throwOnMissingValue: false);
        }
    }

    private static string ExecutablePathForRegistry()
    {
        var path = Environment.ProcessPath
                   ?? System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName
                   ?? "";
        // Quote path so spaces (e.g. "F:\Windows Pinboard\…") survive Run-key parsing.
        return $"\"{path}\"";
    }
}
