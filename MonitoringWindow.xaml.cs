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
            InitializeComponent();
            
            LinkManager linkManager = new LinkManager();
            WorkerManager workerManager = new WorkerManager();
            IncidentManager incidentManager = new IncidentManager();
            
            linkManager.LoadLinks();
            workerManager.InitWorkers(linkManager.LinkDatabase);
            incidentManager.StartWatchers(linkManager.LinkDatabase);

            InitializeComponent();

            view = new LinkView(this, linkManager.LinkDatabase.First().Value);
        }
    }
}
