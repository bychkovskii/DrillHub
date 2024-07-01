using DrillHub.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace DrillHub.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {

        public List<string> PageOptions { get; }

        public ObservableCollection<Node> Nodes { get; }      

        private void TriggerPane()
        {
            //IsPaneOpen = !IsPaneOpen;
        }
    }

}
