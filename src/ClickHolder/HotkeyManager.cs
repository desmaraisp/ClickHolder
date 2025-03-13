using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Win32.Input;

namespace ClickHolder;

public partial class HotkeyManager
{
    private readonly TopLevel window;
    private int currentCallbackId;
    private ConcurrentDictionary<int, Action?> callbacks { get; } = [];
    private const int WM_HOTKEY = 0x0312;
    private const int WM_DESTROY = 0x0002;

    public HotkeyManager(TopLevel window)
    {
        this.window = window;
        RegisterCallback();
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    public void RegisterCombo(Action callback, KeyModifiers keyModifier, IReadOnlyList<Key> keyCodes)
    {
        var nextCallbackId = Interlocked.Increment(ref currentCallbackId);
        var winCombinedKeys = keyCodes.Select(KeyInterop.VirtualKeyFromKey).Aggregate((x, y) => x | y);
        if (RegisterHotKey(GetWindowHandle(), nextCallbackId, (int)keyModifier, winCombinedKeys))
        {
            callbacks.TryAdd(nextCallbackId, callback);
        }
    }

    private IntPtr GetWindowHandle()
    {
        return window.TryGetPlatformHandle()?.Handle ??
               throw new InvalidOperationException("Could not get platform handle");
    }

    public void UnregisterAll()
    {
        foreach (var id in callbacks.Keys)
        {
            UnregisterHotKey(GetWindowHandle(), id);
            callbacks.Remove(id, out _);
        }
    }

    private void RegisterCallback()
    {
        Win32Properties.AddWndProcHookCallback(window, (IntPtr _, uint msg, IntPtr wParam, IntPtr _, ref bool _) =>
        {
            switch (msg)
            {
                case WM_HOTKEY: //raise the HotkeyPressed event
                    if (callbacks?.TryGetValue(wParam.ToInt32(), out var callback) ?? false)
                    {
                        callback?.Invoke();
                    }

                    break;

                case WM_DESTROY: //unregister all hot keys
                    UnregisterAll();
                    break;
            }

            return IntPtr.Zero;
        });
    }
}