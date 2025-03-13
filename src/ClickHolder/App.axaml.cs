using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Win32.Input;

namespace ClickHolder;

public partial class HotkeyManager
{
    private readonly TopLevel window;
    private int nextCallbackId;
    private Dictionary<int, Action?> callbacks { get; } = [];
    private const int WM_HOTKEY = 0x0312;
    private const int WM_DESTROY = 0x0002;

    public HotkeyManager(TopLevel window)
    {
        this.window = window;
        RegisterCallback();
    }
    
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr hWnd, int id, int keyModifiers, int keyCodes);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    public void RegisterCombo(Action callback, KeyModifiers keyModifier, IReadOnlyList<Key> keyCodes)
    {
        nextCallbackId += 1;
        var winCombinedKeys = keyCodes.Select(KeyInterop.VirtualKeyFromKey).Aggregate((x, y) => x | y);
        if (RegisterHotKey(GetWindowHandle(), nextCallbackId, (int)keyModifier, winCombinedKeys))
        {
            callbacks.Add(nextCallbackId, callback);
        }
    }

    private IntPtr GetWindowHandle()
    {
        return window.TryGetPlatformHandle()?.Handle ?? throw new InvalidOperationException("Could not get platform handle");
    }

    public void UnregisterAll()
    {
        foreach (var id in callbacks.Keys)
        {
            UnregisterHotKey(GetWindowHandle(), id);
        }
    }

    private void RegisterCallback()
    {
        Win32Properties.AddWndProcHookCallback(window, (IntPtr _, uint msg, IntPtr wParam, IntPtr _, ref bool _) =>
        {
            switch (msg)
            {
                case WM_HOTKEY: //raise the HotkeyPressed event
                    callbacks?.GetValueOrDefault(wParam.ToInt32(), null)?.Invoke();
                    break;

                case WM_DESTROY: //unregister all hot keys
                    var windowHandle = window.TryGetPlatformHandle()?.Handle;
                    if (windowHandle == null) break;
                    UnregisterAll();
                    break;
            }

            return IntPtr.Zero;
        });
    }
}

public class App : Application
{
    private HotkeyManager hotkeyManager = null!;

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.MainWindow is null)
            {
                return;
            }
            hotkeyManager = new HotkeyManager(desktop.MainWindow);

            hotkeyManager.RegisterCombo(() => { }, KeyModifiers.Alt | KeyModifiers.Control, [Key.S, Key.A]);
            hotkeyManager.RegisterCombo(() => { }, KeyModifiers.Alt, [Key.K]);

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