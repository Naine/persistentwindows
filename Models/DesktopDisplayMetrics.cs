using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var sb = new StringBuilder();
            foreach (var m in displays.OrderBy(row => (((ulong)(uint)row.Left) << 32) | (uint)row.Top))
            {
                if (sb.Length == 0) sb.Append(',');
                sb.Append("[Loc:").Append(m.Left).Append('x').Append(m.Top).Append(" Res:")
                    .Append(m.ScreenWidth).Append('x').Append(m.ScreenHeight).Append(']');
            }
            Key = sb.ToString();
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
