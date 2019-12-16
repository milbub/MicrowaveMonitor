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

namespace MicrowaveMonitor
{
    public partial class MainWindow : Window
    {
        private LinkView view;

        public MainWindow()
        {          
            LinkManager linkManager = new LinkManager();
            WorkerManager workerManager = new WorkerManager();
            IncidentManager incidentManager = new IncidentManager();
            
            linkManager.LoadLinks();
            workerManager.InitWorkers(linkManager.LinkDatabase);
            incidentManager.StartWatchers(linkManager.LinkDatabase);

            InitializeComponent();

            siteA.Checked += SiteChoosed;
            siteB.Checked += SiteChoosed;
            siteR1.Checked += SiteChoosed;
            siteR2.Checked += SiteChoosed;
            siteR3.Checked += SiteChoosed;
            siteR4.Checked += SiteChoosed;

            view = new LinkView(this, linkManager.LinkDatabase.First().Value);
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

        private void SiteChoosed(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb.IsChecked.Value)
                view.ChangeDevice(rb.Content.ToString());
        }
    }
}
