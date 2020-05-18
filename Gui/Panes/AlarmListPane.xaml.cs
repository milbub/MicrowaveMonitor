using MicrowaveMonitor.Managers;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MicrowaveMonitor.Gui
{
    public partial class AlarmListPane : UserControl
    {
        private AlarmManager alarmM;
        private LinkManager linkM;
        private MonitoringWindow window;

        public AlarmListPane()
        {
            InitializeComponent();
        }

        public void SetItemsSource(AlarmManager alarmM, LinkManager linkM, MonitoringWindow window)
        {
            viewCurrent.ItemsSource = alarmM.alarmsCurrent;
            viewAck.ItemsSource = alarmM.alarmsAck;
            viewSettledAck.ItemsSource = alarmM.alarmsSettledAck;
            viewSettledUnack.ItemsSource = alarmM.alarmsSettledUnack;
            this.alarmM = alarmM;
            this.linkM = linkM;
            this.window = window;
        }

        private void AckCheckFired(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            alarmM.SetAck((int)box.Tag, true, true);
        }

        private void AckUncheckFired(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            alarmM.SetAck((int)box.Tag, true, false);
        }

        private void SettlAckCheckFired(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            alarmM.SetAck((int)box.Tag, false, true);
        }

        private void SettlAckUncheckFired(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            alarmM.SetAck((int)box.Tag, false, false);
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

        private void AlarmRowSelected(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AlarmManager.AlarmDisplay disp = (AlarmManager.AlarmDisplay)((ListViewItem)sender).Content;
            window.ChangeLink(linkM.GetLink(linkM.LinkNames.FirstOrDefault(x => x.Value == disp.Link).Key), disp.Device);
            window.SelectNameInLinksList();
        }
    }
}
