using MicrowaveMonitor.Managers;
using System.Windows;
using System.Windows.Controls;

namespace MicrowaveMonitor.Gui
{
    public partial class AlarmListPane : UserControl
    {
        private AlarmManager alarmM;

        public AlarmListPane()
        {
            InitializeComponent();
        }

        public void SetItemsSource(AlarmManager alarmM)
        {
            viewCurrent.ItemsSource = alarmM.alarmsCurrent;
            viewAck.ItemsSource = alarmM.alarmsAck;
            viewSettledAck.ItemsSource = alarmM.alarmsSettledAck;
            viewSettledUnack.ItemsSource = alarmM.alarmsSettledUnack;
            this.alarmM = alarmM;
        }

        private void AckCheckFired(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            alarmM.SetAck((int)box.Tag, true);
        }

        private void AckUncheckFired(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            alarmM.UnsetAck((int)box.Tag, true);
        }

        private void SettlAckCheckFired(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            alarmM.SetAck((int)box.Tag, false);
        }

        private void SettlAckUncheckFired(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            alarmM.UnsetAck((int)box.Tag, false);
        }

        private void HideButtonFired(object sender, RoutedEventArgs e)
        {
            Button butt = (Button)sender;
            if (butt.Name == "bUn")
                alarmM.HideAlarm((int)butt.Tag, false);
            else
                alarmM.HideAlarm((int)butt.Tag, true);
        }

        private void HideAllButtonFired(object sender, RoutedEventArgs e)
        {
            Button butt = (Button)sender;
            if (butt.Name == "bUnAll")
                alarmM.HideAll(false);
            else
                alarmM.HideAll(true);
        }
    }
}
