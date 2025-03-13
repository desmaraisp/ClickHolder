using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;

namespace ClickHolder;

public class App : Application
{
    private HotkeyManager hotkeyManager = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Window
            {
                ShowInTaskbar = false,
                ShowActivated = false,
                Position = new PixelPoint(-10024324, -4624542), 
                IsTabStop = false,
                Focusable = false,
                IsVisible = false,
                IsEnabled = false
            };
            
            hotkeyManager = new HotkeyManager(desktop.MainWindow);
            var clickHolder = new ClickHolder();

            hotkeyManager.RegisterCombo(
                () => { clickHolder.Toggle(); },
                KeyModifiers.Alt,
                [Key.K]);

            // Prevent automatic shutdown
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            desktop.Exit += (_, _) => hotkeyManager.UnregisterAll();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnExitButtonClick(object? sender, EventArgs e)
    {
        Environment.Exit(0);
    }
}