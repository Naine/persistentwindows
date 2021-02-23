using System;
using System.Runtime.InteropServices;

// TODO use raw function pointers

namespace Ninjacrab.PersistentWindows.WinApiBridge
{
    internal unsafe class Interop
    {
        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern BOOL EnumDisplayMonitors(
            HDC hdc, /*const*/ RECT* lprcClip, MONITORENUMPROC lpfnEnum, nint dwData);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern BOOL EnumWindows(WNDENUMPROC lpEnumFunc, nint lParam);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern uint GetDpiForWindow(HWND hwnd);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern BOOL GetMonitorInfoW(HMONITOR hMonitor, MONITORINFO* lpmi);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern HWND GetParent(HWND hWnd);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern BOOL GetWindowPlacement(HWND hWnd, WINDOWPLACEMENT* lpwndpl);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern BOOL GetWindowRect(HWND hWnd, RECT* lpRect);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true,
            CharSet = CharSet.Unicode)]
        public static extern int GetWindowTextW(HWND hWnd, char* lpString, int nMaxCount);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int GetWindowTextLengthW(HWND hWnd);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern uint GetWindowThreadProcessId(HWND hWnd, uint* lpdwProcessId);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern BOOL IsWindowVisible(HWND hWnd);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true,
            SetLastError = true)]
        public static extern BOOL SetWindowPlacement(HWND hWnd, /*const*/ WINDOWPLACEMENT* lpwndpl);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true,
            CharSet = CharSet.Unicode)]
        public static extern BOOL SetWindowTextW(HWND hWnd, /*const*/ char* lpString);
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal unsafe delegate BOOL MONITORENUMPROC(
        HMONITOR hMonitor, HDC hdc, RECT* lpRect, nint dwData);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate BOOL WNDENUMPROC(HWND hWnd, nint lParam);

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct BOOL
    {
        public readonly int Value;

        private BOOL(int value)
            => Value = value;

        public static implicit operator bool(BOOL b)
            => b.Value != 0;

        public static implicit operator BOOL(bool b)
            => new BOOL(b ? 1 : 0);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct HDC
    {
        public readonly IntPtr Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct HMONITOR
    {
        public readonly IntPtr Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct HWND
    {
        public readonly IntPtr Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width()
            => Right - Left;

        public int Height()
            => Bottom - Top;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MONITORINFO
    {
        public uint StructureSize;
        public RECT Monitor;
        public RECT WorkArea;
        public uint Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WINDOWPLACEMENT
    {
        public uint Length;
        public uint Flags;
        public uint ShowCmd;
        public POINT MinPosition;
        public POINT MaxPosition;
        public RECT NormalPosition;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct POINT
    {
        public readonly int X;
        public readonly int Y;
    }
}
