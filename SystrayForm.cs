using System;
using System.Windows.Forms;

namespace Ninjacrab.PersistentWindows
{
    public partial class SystrayForm : Form
    {
        private readonly PersistentWindowProcessor m_persistentWindowProcessor;

        public SystrayForm(PersistentWindowProcessor persistentWindowProcessor)
        {
            m_persistentWindowProcessor = persistentWindowProcessor;
            InitializeComponent();
            Hide();
        }

        protected override void SetVisibleCore(bool value)
        {
            // After the form is constructed and loaded, Application.Run()
            // will explicitly make the form visible, so it is not sufficient
            // to call Hide() in the ctor or even in the form Load handler.
            // By overriding this, we intercept any attempt to show the form.
            base.SetVisibleCore(false);
        }

        private void ExitToolStripMenuItemClickHandler(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
