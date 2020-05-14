using MicrowaveMonitor.Managers;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace MicrowaveMonitor.Gui
{
    public partial class DeviceAlarmListPane : UserControl
    {
        public DeviceAlarmListPane()
        {
            InitializeComponent();
        }

        public void SetItemsSource(ObservableCollection<AlarmManager.AlarmDisplay> alarmsSource)
        {
            view.ItemsSource = alarmsSource;
        }
    }
}
