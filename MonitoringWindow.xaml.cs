using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using MicrowaveMonitor.Database;
using MicrowaveMonitor.Managers;
using MicrowaveMonitor.Workers;
using MicrowaveMonitor.Interface;
using Lextm.SharpSnmpLib;
using System.ComponentModel;

namespace MicrowaveMonitor
{
    public partial class MonitoringWindow : Window
    {
        private LinkManager linkManager;
        private WorkerManager workerManager;
        private IncidentManager incidentManager;
        private LinkView view;

        public MonitoringWindow(LinkManager linkManager, WorkerManager workerManager, IncidentManager incidentManager)
        {
            this.linkManager = linkManager;
            this.workerManager = workerManager;
            this.incidentManager = incidentManager;

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

            view = new LinkView(this, linkManager.LinkDatabase.First().Value);
        }

        private void SiteChoosed(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb.IsChecked.Value)
                view.ChangeDevice(rb.Content.ToString());
        }

        void LinkChoosed(object sender, SelectionChangedEventArgs e)
        {
            view.ChangeLink(linkManager.LinkDatabase[(string)LinksList.SelectedItem]);
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
        }

        public void SiteChooserEnabler(bool state, RadioButton rb)
        {
            rb.IsEnabled = state;
        }
    }
}
