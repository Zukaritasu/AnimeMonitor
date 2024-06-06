using AnimeMonitor.API.Jimov;
using AnimeMonitor.Components;
using AnimeMonitor.Services;
using AnimeMonitor.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnimeMonitor
{
    internal static class App
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (Mutex mutex = new Mutex(true, "c8b7f6e2a5d3e6f0e4b9a2a3a3e2e4e1", out bool isNewInstance)) {
                if (isNewInstance) {
                    try {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.ApplicationExit += (sender, e) => LoadNotifier.GetInstance().StopMonitor();
                        AppTrayMenu.GetInstance().InitTray();
                        LoadNotifier.GetInstance().RunMonitor();
                        Downloads.GetInstance().InitService();
                        Application.Run();
                    } catch (Exception e) {
                        Logs.CreateReport(e);
                    }
                }
            }
        }
    }
}
