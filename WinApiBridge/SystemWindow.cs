using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Ninjacrab.PersistentWindows.WinApiBridge
{
    internal readonly struct SystemWindow
    {
        public static List<SystemWindow> AllToplevelWindows
        {
            get
            {
                var wnds = new List<SystemWindow>();
                Interop.EnumWindows((hWnd, _) =>
                {
                    wnds.Add(new SystemWindow(hWnd));
                    return true;
                }, 0);
                return wnds;
            }
        }

        public HWND HWnd { get; }

        public SystemWindow(HWND HWnd)
        {
            this.HWnd = HWnd;
        }

        public SystemWindow Parent
            => new SystemWindow(Interop.GetParent(HWnd));

        public unsafe Process Process
        {
            get
            {
                int pid;
                _ = Interop.GetWindowThreadProcessId(HWnd, (uint*)&pid);
                return Process.GetProcessById(pid);
            }
        }

        public unsafe string Title
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                int len = Interop.GetWindowTextLengthW(HWnd) + 1;
                if (len > 1)
                {
                    fixed (char* pbuffer = len <= 64 ? stackalloc char[len] : new char[len])
                    {
                        if ((len = Interop.GetWindowTextW(HWnd, pbuffer, len)) > 0)
                        {
                            return new string(pbuffer, 0, len);
                        }
                    }
                }
                return string.Empty;
            }
            set
            {
                fixed (char* p = value)
                {
                    Interop.SetWindowTextW(HWnd, p);
                }
            }
        }

        public bool Visible
            => Interop.IsWindowVisible(HWnd);
    }
}
