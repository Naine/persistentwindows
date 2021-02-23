using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using Ninjacrab.PersistentWindows.Diagnostics;
using Ninjacrab.PersistentWindows.Models;
using Ninjacrab.PersistentWindows.WinApiBridge;

namespace Ninjacrab.PersistentWindows
{
    public class PersistentWindowProcessor
    {
        private const double NATIVE_DPI = 96;

        // read and update this from a config file eventually
        private const int AppsMovedThreshold = 4;
        private DesktopDisplayMetrics? lastMetrics = null;

        public void Start()
        {
            lastMetrics = DesktopDisplayMetrics.AcquireMetrics();
            CaptureApplicationsOnCurrentDisplays(initialCapture: true);

            var thread = new Thread(InternalRun)
            {
                IsBackground = true,
                Name = "PersistentWindowProcessor.InternalRun()"
            };
            thread.Start();

            SystemEvents.DisplaySettingsChanged += (s, e) =>
                {
                    Log.Info("Display settings changed");
                    BeginRestoreApplicationsOnCurrentDisplays();
                };
            SystemEvents.PowerModeChanged += (s, e) =>
            {
                switch (e.Mode)
                {
                    case PowerModes.Suspend:
                        Log.Info("System Suspending");
                        BeginCaptureApplicationsOnCurrentDisplays();
                        break;

                    case PowerModes.Resume:
                        Log.Info("System Resuming");
                        BeginRestoreApplicationsOnCurrentDisplays();
                        break;
                }
            };
        }

        private readonly Dictionary<string, SortedDictionary<string, ApplicationDisplayMetrics>> monitorApplications = new();
        private readonly object displayChangeLock = new object();

        private void InternalRun()
        {
            while (true)
            {
                CaptureApplicationsOnCurrentDisplays();
                Thread.Sleep(1000);
            }
        }

        private void BeginCaptureApplicationsOnCurrentDisplays()
        {
            var thread = new Thread(() => CaptureApplicationsOnCurrentDisplays())
            {
                IsBackground = true,
                Name = "PersistentWindowProcessor.BeginCaptureApplicationsOnCurrentDisplays()"
            };
            thread.Start();
        }

        private void CaptureApplicationsOnCurrentDisplays(string? displayKey = null, bool initialCapture = false)
        {            
            lock(displayChangeLock)
            {
                DesktopDisplayMetrics metrics = DesktopDisplayMetrics.AcquireMetrics();
                if (displayKey == null)
                {
                    displayKey = metrics.Key;
                }

                if (!metrics.Equals(lastMetrics))
                {
                    // since the resolution doesn't match, lets wait till it's restored
                    Log.Info("Detected changes in display metrics, will capture once windows are restored");
                    return;
                }

                if (!monitorApplications.ContainsKey(displayKey))
                {
                    monitorApplications.Add(displayKey, new SortedDictionary<string, ApplicationDisplayMetrics>());
                }

                var appWindows = CaptureWindowsOfInterest();

                List<string> changeLog = new List<string>();
                List<ApplicationDisplayMetrics> apps = new List<ApplicationDisplayMetrics>();
                foreach (var window in appWindows)
                {
                    bool addToChangeLog = AddOrUpdateWindow(displayKey, window, out var metric);

                    if (addToChangeLog)
                    {
                        apps.Add(metric);
                        var pos = metric.WindowPlacement.NormalPosition;
                        changeLog.Add($"CAOCD - Capturing {metric,-45} at [{pos.Left,4}x{pos.Top,4}]"
                            + $" size [{pos.Width(),4}x{pos.Height(),4}] V:{window.Visible} {window.Title} ");
                    #if DEBUG
                        Log.Info(changeLog.Last());
                    #endif
                    }
                }

                // only save the updated if it didn't seem like something moved everything
                if ((apps.Count > 0 && apps.Count < AppsMovedThreshold) || initialCapture)
                {
                    foreach (var app in apps)
                    {
                        if (!monitorApplications[displayKey].ContainsKey(app.Key))
                        {
                            monitorApplications[displayKey].Add(app.Key, app);
                        }
                        else if (!monitorApplications[displayKey][app.Key].EqualPlacement(app))
                        {
                            monitorApplications[displayKey][app.Key].WindowPlacement = app.WindowPlacement;
                        }
                    }
                    changeLog.Sort();
                    Log.Info($"{(initialCapture ? "Initial " : "")}Capturing applications for {displayKey}");
                    Log.Trace($"{apps.Count} windows recorded{Environment.NewLine}{string.Join(Environment.NewLine, changeLog)}");
                }
            }
        }

        private static IEnumerable<SystemWindow> CaptureWindowsOfInterest()
        {
            return SystemWindow.AllToplevelWindows
                                .Where(row => (nint)row.Parent.HWnd.Value == 0
                                    && !string.IsNullOrEmpty(row.Title)
                                    && !row.Title.Equals("Program Manager")
                                    && row.Visible);
        }

        private const uint SW_SHOWNORMAL = 1;
        private const uint SW_MAXIMIZE = 3;

        private unsafe bool AddOrUpdateWindow(string displayKey, SystemWindow window, out ApplicationDisplayMetrics applicationDisplayMetric)
        {
            WINDOWPLACEMENT windowPlacement;
            windowPlacement.Length = (uint)sizeof(WINDOWPLACEMENT);
            Interop.GetWindowPlacement(window.HWnd, &windowPlacement);

            if (windowPlacement.ShowCmd == SW_SHOWNORMAL)
            {
                Interop.GetWindowRect(window.HWnd, &windowPlacement.NormalPosition);
                
                // Undo windows scale factor in window size or we end up inflating the size of windows
                double dpi = Interop.GetDpiForWindow(window.HWnd);
                ref var pos = ref windowPlacement.NormalPosition;
                pos.Right = pos.Left + (int)((pos.Right - pos.Left) / dpi * NATIVE_DPI);
                pos.Bottom = pos.Top + (int)((pos.Bottom - pos.Top) / dpi * NATIVE_DPI);
            }

            applicationDisplayMetric = new ApplicationDisplayMetrics(
#if DEBUG
                // Fetching these is super CPU intensive so do it on debug builds only
                window.Process.Id, window.Process.ProcessName)
#else
                0, "...")
#endif
            {
                HWnd = window.HWnd,
                WindowPlacement = windowPlacement
            };

            bool updated = false;
            if (!monitorApplications[displayKey].TryGetValue(applicationDisplayMetric.Key, out var value)
                || !value.EqualPlacement(applicationDisplayMetric))
            {
                updated = true;
            }
            return updated;
        }

        public void BeginRestoreApplicationsOnCurrentDisplays()
        {
            var thread = new Thread(() =>
            {
                try
                {
                    RestoreApplicationsOnCurrentDisplays();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            })
            {
                IsBackground = true,
                Name = "PersistentWindowProcessor.RestoreApplicationsOnCurrentDisplays()"
            };
            thread.Start();
        }

        private unsafe void RestoreApplicationsOnCurrentDisplays(string? displayKey = null)
        {
            lock (displayChangeLock)
            {
                DesktopDisplayMetrics metrics = DesktopDisplayMetrics.AcquireMetrics();
                if (displayKey == null)
                {
                    displayKey = metrics.Key;
                }

                lastMetrics = DesktopDisplayMetrics.AcquireMetrics();
                if (!monitorApplications.ContainsKey(displayKey))
                {
                    // no old profile, we're done
                    Log.Info($"No old profile found for {displayKey}");
                    CaptureApplicationsOnCurrentDisplays(initialCapture: true);
                    return;
                }

                Log.Info($"Restoring applications for {displayKey}");
                foreach (var window in CaptureWindowsOfInterest())
                {
                    string applicationKey = window.HWnd.Value.ToString();
                    if (monitorApplications[displayKey].ContainsKey(applicationKey))
                    {
                        // looks like the window is still here for us to restore
                        WINDOWPLACEMENT windowPlacement = monitorApplications[displayKey][applicationKey].WindowPlacement;

                        if (windowPlacement.ShowCmd == SW_MAXIMIZE)
                        {
                            // When restoring maximized windows, it occasionally switches res and when the maximized setting is restored
                            // the window thinks it's maxxed, but does not eat all the real estate. So we'll temporarily unmaximize then
                            // re-apply that
                            windowPlacement.ShowCmd = SW_SHOWNORMAL;
                            Interop.SetWindowPlacement(monitorApplications[displayKey][applicationKey].HWnd, &windowPlacement);
                            windowPlacement.ShowCmd = SW_MAXIMIZE;
                        }
                        var success = Interop.SetWindowPlacement(monitorApplications[displayKey][applicationKey].HWnd, &windowPlacement);
                        if(!success)
                        {
                            Log.Error(new Win32Exception(Marshal.GetLastWin32Error()).Message);
                        }

                        ref var pos = ref windowPlacement.NormalPosition;
                        string pName = window.Process.ProcessName;
                        Log.Info($"SetWindowPlacement({pName} [{pos.Left}x{pos.Top}]-[{pos.Width()}x{pos.Height()}]) - {success}");
                    }
                }
            }
        }
    }
}
