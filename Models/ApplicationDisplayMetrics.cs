using System;
using Ninjacrab.PersistentWindows.WinApiBridge;

namespace Ninjacrab.PersistentWindows.Models
{
    internal class ApplicationDisplayMetrics
    {
        public HWND HWnd { get; set; }
        public int ProcessId { get; set; }
        public string ApplicationName { get; set; }
        public WINDOWPLACEMENT WindowPlacement { get; set; }

        public ApplicationDisplayMetrics(int id, string name)
        {
            ProcessId = id;
            ApplicationName = name;
        }

        public string Key
            => $"{HWnd.Value}";

        public bool EqualPlacement(ApplicationDisplayMetrics other)
        {
            return WindowPlacement.NormalPosition.Left == other.WindowPlacement.NormalPosition.Left
                && WindowPlacement.NormalPosition.Top == other.WindowPlacement.NormalPosition.Top
                && WindowPlacement.NormalPosition.Right == other.WindowPlacement.NormalPosition.Right
                && WindowPlacement.NormalPosition.Bottom == other.WindowPlacement.NormalPosition.Bottom;
        }

        public override string ToString()
            => $"{ProcessId}.{HWnd.Value} {ApplicationName}";
    }
}
