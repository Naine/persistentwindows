using System.Collections.Generic;
using System.Linq;
using Ninjacrab.PersistentWindows.WinApiBridge;

namespace Ninjacrab.PersistentWindows.Models
{
    public class DesktopDisplayMetrics
    {
        public static DesktopDisplayMetrics AcquireMetrics()
        {
            return new DesktopDisplayMetrics(Display.GetDisplays());
        }

        public DesktopDisplayMetrics(List<Display> displays)
        {
            List<string> segments = new List<string>();
            foreach (var m in displays.OrderBy(row => row.DeviceName))
            {
                segments.Add($"[DeviceName:{m.DeviceName} Loc:{m.Left}x{m.Top} Res:{m.ScreenWidth}x{m.ScreenHeight}]");
            }
            Key = string.Join(",", segments);
        }

        public string Key { get; }

        public override bool Equals(object? obj)
        {
            return obj is DesktopDisplayMetrics other && Key == other.Key;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
