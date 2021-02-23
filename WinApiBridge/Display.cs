using System;
using System.Collections.Generic;

namespace Ninjacrab.PersistentWindows.WinApiBridge
{
    public class Display
    {
        public int ScreenWidth { get; }
        public int ScreenHeight { get; }
        public int Left { get; }
        public int Top { get; }
        public uint Flags { get; }

        private Display(in MONITORINFO monitorInfo)
        {
            ScreenWidth = monitorInfo.Monitor.Right - (Left = monitorInfo.Monitor.Left);
            ScreenHeight = monitorInfo.Monitor.Bottom - (Top = monitorInfo.Monitor.Top);
            Flags = monitorInfo.Flags;
        }

        public static unsafe List<Display> GetDisplays()
        {
            List<Display> displays = new List<Display>();

            Interop.EnumDisplayMonitors(default, null, (hMonitor, _, _, _) =>
                {
                    MONITORINFO mi;
                    mi.StructureSize = (uint)sizeof(MONITORINFO);
                    if (Interop.GetMonitorInfoW(hMonitor, &mi))
                    {
                        displays.Add(new Display(mi));
                    }
                    return true;
                }, 0);
            return displays;
        }
    }
}
