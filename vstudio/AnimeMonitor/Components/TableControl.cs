using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnimeMonitor.Components {
    internal class TableControl : ListView {

        public TableControl() {
            DoubleBuffered = true;
            FullRowSelect = true;
            Font = SystemFonts.MenuFont;
        }
    }
}
