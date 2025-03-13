using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace ClickHolder;

[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int X;
    public int Y;
}

public partial class ClickHolder
{
    private bool isHolding;
    private readonly Lock isHoldingLock = new();

    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;

    [LibraryImport("user32.dll")]
    private static partial void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorPos(out POINT lpPoint);

    public void Toggle()
    {
        lock (isHoldingLock)
        {
            if (isHolding)
            {
                Console.WriteLine("Clack");
                if (!GetCursorPos(out var lpPoint))
                {
                    return;
                }

                mouse_event(MOUSEEVENTF_LEFTUP, lpPoint.X, lpPoint.Y, 0, 0);
                isHolding = false;
            }

            else
            {
                Console.WriteLine("Click");
                if (!GetCursorPos(out var lpPoint))
                {
                    return;
                }

                mouse_event(MOUSEEVENTF_LEFTDOWN, lpPoint.X, lpPoint.Y, 0, 0);
                isHolding = true;
            }
        }
    }
}