using System.ComponentModel;
using Prism.Mvvm;

namespace Ninjacrab.PersistentWindows.WpfShell
{
    public class DiagnosticsViewModel : BindableBase
    {
        public DiagnosticsViewModel()
        {
            allProcesses = new BindingList<string>();
            RaisePropertyChanged(nameof(EventLog));
        }

        public const string AllProcessesPropertyName = "AllProcesses";
        private BindingList<string> allProcesses;
        public BindingList<string> EventLog
        {
            get { return allProcesses; }
            set { SetProperty(ref allProcesses, value); } 
        }

    }
}
