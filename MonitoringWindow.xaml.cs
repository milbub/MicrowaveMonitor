using MicrowaveMonitor.Interface;
using MicrowaveMonitor.Managers;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MicrowaveMonitor
{
    public partial class MonitoringWindow : Window
    {
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

            siteA.Checked += SiteChoosed;
            siteB.Checked += SiteChoosed;
            siteR1.Checked += SiteChoosed;
            siteR2.Checked += SiteChoosed;
            siteR3.Checked += SiteChoosed;
            siteR4.Checked += SiteChoosed;

            LinksList.ItemsSource = linkManager.LinkDatabase.Keys;
            LinksList.SelectedItem = linkManager.LinkDatabase.First().Key;
            LinksList.SelectionChanged += LinkChoosed;
            AlarmsList.ItemsSource = alarmManager.Alarms;

            view = new LinkView(this, linkManager.LinkDatabase.First().Value);
            settings = new LinkSettings(this, view);
        }

        private void SiteChoosed(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb.IsChecked.Value)
                view.ChangeDevice(rb.Content.ToString());
        }

        private void LinkChoosed(object sender, SelectionChangedEventArgs e)
        {
            view.ChangeLink(linkManager.LinkDatabase[(string)LinksList.SelectedItem]);
            settings.ChangeSettings();
        }

        public void ResetView()
        {
            ip.Content = String.Empty;
            unitname.Content = String.Empty;
            ping.Content = String.Empty;
            uptime.Content = String.Empty;
            signalLevel.Text = String.Empty;
            signalQuality.Text = String.Empty;
            tx.Text = String.Empty;
            rx.Text = String.Empty;
            pingwin.Text = String.Empty;
        }

        public void SiteChooserEnabler(bool state, RadioButton rb)
        {
            rb.IsEnabled = state;
        }
    }
}
