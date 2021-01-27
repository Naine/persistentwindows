using System;
using Ninjacrab.PersistentWindows.WinApiBridge;

namespace Ninjacrab.PersistentWindows.Models
{
    public class ApplicationDisplayMetrics
    {
        public IntPtr HWnd { get; set; }
        public int ProcessId { get; set; }
        public string ApplicationName { get; set; }
        public WindowPlacement WindowPlacement { get; set; }

        public ApplicationDisplayMetrics(int id, string name)
        {
            ProcessId = id;
            ApplicationName = name;
        }

        public string Key
            => $"{HWnd.ToInt64()}";

        public bool EqualPlacement(ApplicationDisplayMetrics other)
        {
            return this.WindowPlacement.NormalPosition.Left == other.WindowPlacement.NormalPosition.Left
                && this.WindowPlacement.NormalPosition.Top == other.WindowPlacement.NormalPosition.Top
                && this.WindowPlacement.NormalPosition.Width == other.WindowPlacement.NormalPosition.Width
                && this.WindowPlacement.NormalPosition.Height == other.WindowPlacement.NormalPosition.Height;
        }

        public override string ToString()
            => $"{ProcessId}.{HWnd.ToInt64()} {ApplicationName}";
    }
}
