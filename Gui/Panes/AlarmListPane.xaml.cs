using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MicrowaveMonitor.Managers;

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
            alarmM.SetAck((int)box.Tag);
        }

        private void AckUncheckFired(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            alarmM.UnsetAck((int)box.Tag);
        }

        private void HideButtonFired(object sender, RoutedEventArgs e)
        {
            Button butt= (Button)sender;
        }
    }
}
