using System.Threading;
using System.Windows;
using WindowsPinboard.Services;

namespace WindowsPinboard;

public partial class App : Application
{
    private static Mutex? _singletonMutex;
    private MainWindow? _main;
    private TrayService? _tray;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        _singletonMutex = new Mutex(initiallyOwned: true,
            "WindowsPinboard.SingletonMutex.{C7B0F4A2-3E2B-4F0E-9DCD-8B9E7C1A0F11}", out var createdNew);
        if (!createdNew)
        {
            MessageBox.Show("Windows Pinboard is already running.",
                "Windows Pinboard", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        _main = new MainWindow();
        MainWindow = _main;
        _main.Show();

        _tray = new TrayService();
        _tray.ShowRequested += () => _main?.Dispatcher.Invoke(() => _main.ShowAndFocus());
        _tray.SettingsRequested += () => _main?.Dispatcher.Invoke(() => _main.OpenSettingsDialog());
        _tray.ExitRequested += () => Dispatcher.Invoke(Shutdown);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        _singletonMutex?.ReleaseMutex();
        _singletonMutex?.Dispose();
        base.OnExit(e);
    }
}
