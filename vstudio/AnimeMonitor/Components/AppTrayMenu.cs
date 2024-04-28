using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnimeMonitor.Components {
    internal class AppTrayMenu {

        private NotifyIcon notifyIcon = null;
        private static AppTrayMenu _instance;

        public static AppTrayMenu GetInstance() {
            if (_instance == null)
                _instance = new AppTrayMenu();
            return _instance;
        }

        private AppTrayMenu() { }

        public void InitTray() {
            if (notifyIcon != null)
                return;

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = Properties.Resources.favicon;
            notifyIcon.Visible = true;
            notifyIcon.MouseClick += EventMouseClick;
            InitComponents();
        }

        private void InitComponents() {
            ToolStripButton button = new ToolStripButton("Bloquear notificaciones");
            button.CheckOnClick = true;

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Abrir"),
                new ToolStripSeparator(),
                button,
                new ToolStripSeparator(),
                new ToolStripMenuItem("Salir", null, new EventHandler(EventClose))
            });
            notifyIcon.ContextMenuStrip = menu;
        }

        private void EventClose(object sender, EventArgs e) {
            Close();
            Application.Exit();
        }

        private void EventMouseClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                notifyIcon.ContextMenuStrip.Show(Cursor.Position, ToolStripDropDownDirection.AboveLeft);
            }
        }

        public void Notify(string title, string message) {
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText = message;
            notifyIcon.ShowBalloonTip(3000);
        }

        public void Close() {
            if (notifyIcon != null) {
                notifyIcon.Dispose();
                notifyIcon = null;
            }
        }
    }
}
