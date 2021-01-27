using System.Collections.Generic;
using System.Linq;
using Ninjacrab.PersistentWindows.Common.WinApiBridge;

namespace Ninjacrab.PersistentWindows.Common.Models
{
    public class DesktopDisplayMetrics
    {
        public static DesktopDisplayMetrics AcquireMetrics()
        {
            return new DesktopDisplayMetrics(Display.GetDisplays());
        }

        private readonly List<Display> monitorResolutions;

        public int NumberOfDisplays { get { return monitorResolutions.Count; } }

        public DesktopDisplayMetrics(List<Display> displays)
        {
            monitorResolutions = displays;
            List<string> segments = new List<string>();
            foreach (var m in monitorResolutions.OrderBy(row => row.DeviceName))
            {
                segments.Add($"[DeviceName:{m.DeviceName} Loc:{m.Left}x{m.Top} Res:{m.ScreenWidth}x{m.ScreenHeight}]");
            }
            key = string.Join(",", segments);
        }

        private string key;
        public string Key
        {
            get
            {
                return key;
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is DesktopDisplayMetrics other && Key == other.key;
        }

        public override int GetHashCode()
        {
            return key.GetHashCode();
        }

        public int GetHashCode(DesktopDisplayMetrics obj)
        {
            return obj.key.GetHashCode();
        }
    }
}
