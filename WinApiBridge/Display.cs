using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ninjacrab.PersistentWindows.WinApiBridge
{
    public class Display
    {
        public int ScreenWidth { get; internal set; }
        public int ScreenHeight { get; internal set; }
        public int Left { get; internal set; }
        public int Top { get; internal set; }
        public uint Flags { get; internal set; }
        public string DeviceName { get; internal set; }

        private Display(string name)
            => DeviceName = name;

        public static List<Display> GetDisplays()
        {
            List<Display> displays = new List<Display>();

            User32.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                delegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
                {
                    MonitorInfo monitorInfo = new MonitorInfo();
                    monitorInfo.StructureSize = Marshal.SizeOf(monitorInfo);
                    bool success = User32.GetMonitorInfo(hMonitor, ref monitorInfo);
                    if (success)
                    {
                        int pos = monitorInfo.DeviceName.LastIndexOf("\\") + 1;
                        displays.Add(new Display(monitorInfo.DeviceName[pos..])
                        {
                            ScreenWidth = monitorInfo.Monitor.Width,
                            ScreenHeight = monitorInfo.Monitor.Height,
                            Left = monitorInfo.Monitor.Left,
                            Top = monitorInfo.Monitor.Top,
                            Flags = monitorInfo.Flags,
                        });
                    }
                    return true;
                }, IntPtr.Zero);
            return displays;
        }
    }
}
