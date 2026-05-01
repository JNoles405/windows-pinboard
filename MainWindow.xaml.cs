using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WindowsPinboard.Models;
using WindowsPinboard.Native;
using WindowsPinboard.Services;
using WindowsPinboard.ViewModels;
using WindowsPinboard.Views;

namespace WindowsPinboard;

public partial class MainWindow : Window
{
    private const int WM_NCHITTEST = 0x0084;
    private const int HTCLIENT = 1;
    private const int HTTRANSPARENT = -1;

    private readonly StorageService _storage;
    private readonly HotkeyService _hotkey;
    private readonly AppBarService _appBar;
    private readonly SidebarViewModel _vm;
    private AppSettings _settings;
    private MonitorInfo _monitor;

    private bool _expanded;
    private bool _pinned;
    private readonly DispatcherTimer _hoverOpenTimer;
    private readonly DispatcherTimer _autoHideTimer;
    private bool _suppressAutoHide;

    public MainWindow()
    {
        InitializeComponent();

        _storage = new StorageService();
        _settings = _storage.LoadSettings();
        _vm = new SidebarViewModel(_storage);
        DataContext = _vm;

        _hotkey = new HotkeyService();
        _hotkey.Pressed += OnHotkeyPressed;

        _appBar = new AppBarService();
        _appBar.PositionChanged += () => Dispatcher.BeginInvoke(new Action(() => ApplyLayout()));

        _monitor = MonitorService.Resolve(_settings.MonitorDeviceName);

        _hoverOpenTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_settings.HoverDelayMs) };
        _hoverOpenTimer.Tick += (_, __) => { _hoverOpenTimer.Stop(); if (!_expanded) Expand(); };

        _autoHideTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_settings.AutoHideDelayMs) };
        _autoHideTimer.Tick += (_, __) => { _autoHideTimer.Stop(); if (!_pinned && _expanded && !IsMouseOverVisibleArea()) Collapse(); };

        Loaded += OnLoaded;
        Closed += OnClosed;

        RootGrid.MouseEnter += (_, __) => OnMouseEnterSidebar();
        RootGrid.MouseLeave += (_, __) => OnMouseLeaveSidebar();
        Deactivated += (_, __) => { if (!_pinned && _expanded) ScheduleAutoHide(); };

        PreviewKeyDown += OnPreviewKeyDown;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        ApplyLayout();
        // Window now sized — install NCHITTEST hook for click-through.
        var hwnd = new WindowInteropHelper(this).Handle;
        HwndSource.FromHwnd(hwnd)?.AddHook(HitTestHook);

        _hotkey.Attach(this);
        TryRegisterHotkey();
        ApplyAppBar();
        UpdatePinVisual();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _appBar.Dispose();
        _hotkey.Dispose();
    }

    private void TryRegisterHotkey()
    {
        _hotkey.Register(_settings.HotkeyModifiers, _settings.HotkeyVirtualKey);
    }

    private void ApplyAppBar()
    {
        if (_settings.RegisterAsAppBar)
        {
            if (!_appBar.IsRegistered) _appBar.Register(this);
            int sliverPx = (int)Math.Round(_settings.CollapsedWidth * _monitor.DpiScale);
            _appBar.SetPosition(_settings.Edge, _monitor, sliverPx);
        }
        else
        {
            _appBar.Unregister();
        }
    }

    /// <summary>
    /// Position window at full expanded size pinned to chosen edge of work area.
    /// Layout doesn't change with collapse/expand — only the inner TranslateTransform animates.
    /// </summary>
    private void ApplyLayout()
    {
        _monitor = MonitorService.Resolve(_settings.MonitorDeviceName);

        var fullWidth = _settings.SidebarWidth + _settings.CollapsedWidth;

        // Configure inner element sizes to match settings (in case they were edited).
        SidebarPanel.Width = _settings.SidebarWidth;
        HandleBorder.Width = _settings.CollapsedWidth;

        if (_settings.Edge == SidebarEdge.Right)
        {
            SidebarPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            SidebarPanel.Margin = new Thickness(0, 0, _settings.CollapsedWidth, 0);
            SidebarPanel.BorderThickness = new Thickness(0, 0, 1, 0);
            HandleBorder.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
        }
        else
        {
            SidebarPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            SidebarPanel.Margin = new Thickness(_settings.CollapsedWidth, 0, 0, 0);
            SidebarPanel.BorderThickness = new Thickness(1, 0, 0, 0);
            HandleBorder.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
        }

        Top = _monitor.WorkTopDip;
        Height = _monitor.WorkHeightDip;
        Width = fullWidth;
        Left = _settings.Edge == SidebarEdge.Right
            ? _monitor.WorkRightDip - fullWidth
            : _monitor.WorkLeftDip;

        // Snap transform to current state (no animation).
        PanelTransform.BeginAnimation(TranslateTransform.XProperty, null);
        PanelTransform.X = ComputeRestX(_expanded);
    }

    private double ComputeRestX(bool expanded)
    {
        // For Edge.Right: panel collapsed slides RIGHT (off the right side of the window).
        // For Edge.Left:  panel collapsed slides LEFT  (off the left side of the window).
        if (expanded) return 0;
        return _settings.Edge == SidebarEdge.Right
            ? _settings.SidebarWidth
            : -_settings.SidebarWidth;
    }

    private void Expand()
    {
        _expanded = true;
        AnimatePanelTo(0);
    }

    private void Collapse()
    {
        if (_pinned) return;
        _expanded = false;
        AnimatePanelTo(ComputeRestX(false));
    }

    private void AnimatePanelTo(double target)
    {
        var dur = new Duration(TimeSpan.FromMilliseconds(_settings.AnimationDurationMs));
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        var anim = new DoubleAnimation
        {
            To = target,
            Duration = dur,
            EasingFunction = ease,
            FillBehavior = FillBehavior.HoldEnd,
        };
        anim.Completed += (_, __) =>
        {
            // Detach the animation so subsequent direct sets work cleanly.
            PanelTransform.BeginAnimation(TranslateTransform.XProperty, null);
            PanelTransform.X = target;
        };
        PanelTransform.BeginAnimation(TranslateTransform.XProperty, anim);
    }

    private void OnHotkeyPressed()
    {
        if (_expanded && _pinned)
        {
            _pinned = false;
            UpdatePinVisual();
            Collapse();
        }
        else if (_expanded)
        {
            _pinned = true;
            UpdatePinVisual();
            Activate();
            FocusEditor();
        }
        else
        {
            Expand();
            _pinned = true;
            UpdatePinVisual();
            Activate();
            FocusEditor();
        }
    }

    public void ShowAndFocus()
    {
        if (!IsVisible) Show();
        if (!_expanded) Expand();
        _pinned = true;
        UpdatePinVisual();
        Activate();
        FocusEditor();
    }

    private void ScheduleAutoHide()
    {
        if (_suppressAutoHide || _pinned) return;
        _autoHideTimer.Stop();
        _autoHideTimer.Start();
    }

    /// <summary>True if the cursor is over a region currently considered "visible" (panel + handle).</summary>
    private bool IsMouseOverVisibleArea()
    {
        var p = Mouse.GetPosition(this);
        if (p.Y < 0 || p.Y > ActualHeight) return false;
        return IsXInVisibleArea(p.X);
    }

    private bool IsXInVisibleArea(double xDip)
    {
        // Visible area shrinks as panel slides away from rest position 0.
        var slide = Math.Abs(PanelTransform.X);
        if (_settings.Edge == SidebarEdge.Right)
        {
            // Visible region is the right portion of the window.
            var visibleLeft = ActualWidth - _settings.CollapsedWidth - (_settings.SidebarWidth - slide);
            return xDip >= visibleLeft - 0.5;
        }
        else
        {
            var visibleRight = _settings.CollapsedWidth + (_settings.SidebarWidth - slide);
            return xDip <= visibleRight + 0.5;
        }
    }

    private IntPtr HitTestHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != WM_NCHITTEST) return IntPtr.Zero;

        // lParam contains the screen coordinates in physical pixels.
        int sx = unchecked((short)((long)lParam & 0xFFFF));
        int sy = unchecked((short)(((long)lParam >> 16) & 0xFFFF));

        var scale = _monitor.DpiScale;
        var screenXDip = sx / scale;
        var screenYDip = sy / scale;

        var clientX = screenXDip - Left;
        var clientY = screenYDip - Top;

        if (clientY < 0 || clientY > ActualHeight)
        {
            handled = true;
            return new IntPtr(HTTRANSPARENT);
        }

        if (IsXInVisibleArea(clientX))
        {
            handled = true;
            return new IntPtr(HTCLIENT);
        }

        handled = true;
        return new IntPtr(HTTRANSPARENT);
    }

    private void OnMouseEnterSidebar()
    {
        _autoHideTimer.Stop();
        if (!_expanded && _settings.HoverToOpen) _hoverOpenTimer.Start();
    }

    private void OnMouseLeaveSidebar()
    {
        _hoverOpenTimer.Stop();
        if (_expanded && !_pinned) ScheduleAutoHide();
    }

    private void OnHandleMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            _pinned = !_pinned;
            UpdatePinVisual();
            if (_pinned && !_expanded) Expand();
            return;
        }

        if (!_expanded) Expand();
        else if (!_pinned) Collapse();
    }

    private void OnAddClick(object sender, RoutedEventArgs e)
    {
        _vm.AddNote();
        FocusEditor();
    }

    private void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (_vm.Selected is { } n) _vm.DeleteNote(n);
    }

    public void OpenSettingsDialog()
    {
        _suppressAutoHide = true;
        try
        {
            var dlg = new SettingsWindow(_settings) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                _settings = dlg.Result;
                _storage.SaveSettings(_settings);

                _hoverOpenTimer.Interval = TimeSpan.FromMilliseconds(_settings.HoverDelayMs);
                _autoHideTimer.Interval = TimeSpan.FromMilliseconds(_settings.AutoHideDelayMs);
                ApplyLayout();
                _hotkey.Unregister();
                TryRegisterHotkey();

                ApplyAppBar();

                try { AutostartService.SetEnabled(_settings.StartWithWindows); }
                catch { /* registry write may fail in restricted environments */ }
            }
        }
        finally
        {
            _suppressAutoHide = false;
        }
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e) => OpenSettingsDialog();

    private void OnPinClick(object sender, RoutedEventArgs e)
    {
        _pinned = !_pinned;
        UpdatePinVisual();
        if (_pinned && !_expanded) Expand();
    }

    private void UpdatePinVisual()
    {
        PinButton.Opacity = _pinned ? 1.0 : 0.55;
        PinButton.ToolTip = _pinned ? "Unpin" : "Pin open";
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (_pinned) { _pinned = false; UpdatePinVisual(); }
            Collapse();
            e.Handled = true;
            return;
        }
        if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
        {
            _vm.AddNote();
            FocusEditor();
            e.Handled = true;
        }
    }

    private void FocusEditor()
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            var box = FindEditorBodyTextBox();
            box?.Focus();
        }), DispatcherPriority.Background);
    }

    private TextBox? FindEditorBodyTextBox()
    {
        var boxes = new List<TextBox>();
        WalkBoxes(this, boxes);
        return boxes.FirstOrDefault(b => b.AcceptsReturn);
    }

    private static void WalkBoxes(System.Windows.DependencyObject root, List<TextBox> acc)
    {
        var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            if (child is TextBox tb) acc.Add(tb);
            WalkBoxes(child, acc);
        }
    }
}
