using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Ninjacrab.PersistentWindows.WinApiBridge
{
    public class SystemWindow
    {
        private delegate int EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private enum BOOL : int
        {
            FALSE = 0,
        }

        [DllImport("user32.dll")]
        private static extern BOOL EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, [Out] StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern BOOL SetWindowText(IntPtr hWnd, string lpString);

        [DllImport("user32.dll")]
        private static extern BOOL IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        public static List<SystemWindow> AllToplevelWindows
        {
            get
            {
                var wnds = new List<SystemWindow>();
                EnumWindows((hwnd, _) =>
                {
                    wnds.Add(new SystemWindow(hwnd));
                    return 1;
                }, IntPtr.Zero);
                return wnds;
            }
        }

        public IntPtr HWnd { get; }

        public SystemWindow(IntPtr HWnd)
        {
            this.HWnd = HWnd;
        }

        public SystemWindow Parent
            => new SystemWindow(GetParent(HWnd));

        public Process Process
        {
            get
            {
                GetWindowThreadProcessId(HWnd, out var pid);
                return Process.GetProcessById(pid);
            }
        }

        public string Title
        {
            get
            {
                var sb = new StringBuilder(GetWindowTextLength(HWnd) + 1);
                GetWindowText(HWnd, sb, sb.Capacity);
                return sb.ToString();
            }
            set
            {
                SetWindowText(HWnd, value);
            }
        }

        public bool Visible
            => IsWindowVisible(HWnd) != BOOL.FALSE;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;

        public int Top;

        public int Right;

        public int Bottom;

        public int Height
            => Bottom - Top;

        public int Width
            => Right - Left;

        public Rectangle ToRectangle()
        {
            return Rectangle.FromLTRB(Left, Top, Right, Bottom);
        }

        public static explicit operator RECT(Rectangle rect)
        {
            return new RECT
            {
                Left = rect.Left,
                Top = rect.Top,
                Right = rect.Right,
                Bottom = rect.Bottom,
            };
        }
    }
}
