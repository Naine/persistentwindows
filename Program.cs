using System;
using System.Windows.Forms;

namespace Ninjacrab.PersistentWindows
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var persistentWindowProcessor = new PersistentWindowProcessor();
            persistentWindowProcessor.Start();
            Application.Run(new SystrayForm(persistentWindowProcessor));
        }
    }
}
