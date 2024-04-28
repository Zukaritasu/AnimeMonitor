using AnimeMonitor.Components;
using AnimeMonitor.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnimeMonitor
{
    internal static class App
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ApplicationExit += (object sender, EventArgs e) => {
                LoadNotifier.GetInstance().StopMonitor();
            };

            AppTrayMenu.GetInstance().InitTray();
            LoadNotifier.GetInstance().RunMonitor();
            //if (args.Length > 0 && args[0] == "-appwindow") {
                Application.Run();
            //} else {
               // Application.Run(new Forms.WinApp(args));
            //}
        }
    }
}
