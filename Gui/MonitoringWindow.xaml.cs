using MicrowaveMonitor.Frontend;
using MicrowaveMonitor.Managers;
using MicrowaveMonitor.Database;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace MicrowaveMonitor.Gui
{
    public partial class MonitoringWindow : Window
    {
        public enum DeviceType { Base, End, R1, R2, R3, R4 };
        private LinkManager linkManager;
        private WorkerManager workerManager;
        private AlarmManager alarmManager;
        
        private LinkView view;
        private LinkSettings settings;

        public MonitoringWindow(LinkManager linkManager, WorkerManager workerManager, AlarmManager alarmManager)
        {
            this.linkManager = linkManager;
            this.workerManager = workerManager;
            this.alarmManager = alarmManager;

            InitializeComponent();

            LinksList.ItemsSource = linkManager.LinkNames.Values;
            LinksList.SelectedItem = linkManager.LinkNames.First().Value;
            view = new LinkView(this, linkManager.LinkDatabase.Get<Link>(linkManager.LinkNames.First().Key), workerManager.DeviceToFront);
            settings = new LinkSettings(this, view);
            
            alarmListPane.AlarmsList.ItemsSource = alarmManager.Alarms;

            LinksList.SelectionChanged += LinkChoosed;

            siteA.Checked += SiteChoosed;
            siteB.Checked += SiteChoosed;
            siteR1.Checked += SiteChoosed;
            siteR2.Checked += SiteChoosed;
            siteR3.Checked += SiteChoosed;
            siteR4.Checked += SiteChoosed;

            signalLevel.axisY.Title = "[dBm]";
            signalQuality.axisY.Title = "[dB]";
            tx.axisY.Title = "[bit/s]";
            rx.axisY.Title = "[bit/s]";
            tx.axisY.MinValue = 0;
            rx.axisY.MinValue = 0;
            tempIdu.axisY.Title = "[°C]";
            tempOdu.axisY.Title = "[°C]";
            voltage.axisY.Title = "[V]";
            voltage.axisY.MinValue = 0;
            pingwin.axisY.Title = "[ms]";
            pingwin.axisY.MinValue = 0;
        }

        public Device GetDevice(int id)
        {
            return linkManager.GetDevice(id);
        }

        public void UpdateElementContent(ContentControl element, string value)
        {
            try
            {
                if (!element.Dispatcher.CheckAccess())
                {
                    element.Dispatcher.Invoke(() =>
                    {
                        element.Content = value;
                    });
                }
                else
                {
                    element.Content = value;
                }
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void UpdateImage(Image element, ImageSource source)
        {
            try
            {
                if (!element.Dispatcher.CheckAccess())
                {
                    element.Dispatcher.Invoke(() =>
                    {
                        element.Source = source;
                    });
                }
                else
                {
                    element.Source = source;
                }
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void GraphUpdate(GraphRealtime element, Record<double> record)
        {
            try
            {
                if (!element.Dispatcher.CheckAccess())
                {
                    element.Dispatcher.Invoke(() =>
                    {
                        element.Read(record);
                    });
                }
                else
                {
                    element.Read(record);
                }
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void SiteChoosed(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb.IsChecked.Value)
                view.ChangeDevice(rb.Content.ToString());
        }

        private void LinkChoosed(object sender, SelectionChangedEventArgs e)
        {
            view.ChangeLink(linkManager.LinkDatabase.Get<Link>(linkManager.LinkNames.FirstOrDefault(x => x.Value == (string)LinksList.SelectedItem).Key));
            settings.ChangeSettings();
        }

        public void SiteChooserEnabler(bool state, RadioButton rb)
        {
            rb.IsEnabled = state;
        }

        public void ResetView()
        {
            ip.Content = String.Empty;
            unitname.Content = String.Empty;
            ping.Content = String.Empty;
            uptime.Content = String.Empty;
            signalLevel.ChartValues.Clear();
            signalQuality.ChartValues.Clear();
            tx.ChartValues.Clear();
            rx.ChartValues.Clear();
            tempIdu.ChartValues.Clear();
            tempOdu.ChartValues.Clear();
            voltage.ChartValues.Clear();
            pingwin.ChartValues.Clear();
        }
    }
}
