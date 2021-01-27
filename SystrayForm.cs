using System;
using System.Windows.Forms;
using Ninjacrab.PersistentWindows.Common;

namespace Ninjacrab.PersistentWindows.SystrayShell
{
    public partial class SystrayForm : Form
    {
        private PersistentWindowProcessor m_persistentWindowProcessor;

        public SystrayForm(PersistentWindowProcessor persistentWindowProcessor)
        {
            m_persistentWindowProcessor = persistentWindowProcessor;
            InitializeComponent();
        }

        private void ExitToolStripMenuItemClickHandler(object sender, EventArgs e)
        {
            this.notifyIconMain.Visible = false;
            this.notifyIconMain.Icon = null;
            Application.Exit();
        }

    }
}
