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
using MicrowaveMonitor.Workers;
using MicrowaveMonitor.Interface;
using Lextm.SharpSnmpLib;

namespace MicrowaveMonitor
{
    public partial class MainWindow : Window
    {
        LinkView view;

        public MainWindow()
        {
            InitializeComponent();

            Device test1 = new Device("10.248.16.64", 161, "public");
            test1.OidUptime = new ObjectIdentifier("1.3.6.1.2.1.1.3.0");
            test1.OidSignalLevel = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.5.0");
            test1.SignalLevelRefresh = 1000;
            test1.OidSignalQuality = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.6.0");
            test1.SignalQualityRefresh = 1000;
            test1.OidTxDataRate = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.14.0");
            test1.TxRefresh = 1000;
            test1.OidRxDataRate = new ObjectIdentifier("1.3.6.1.4.1.23688.1.1.15.0");
            test1.RxRefresh = 1000;

            Link testLinka = new Link("TEST Summit QAM - 10.248.16.64 <-> 10.248.16.65", test1);

            SnmpDataUptime kolektor = new SnmpDataUptime(test1);
            SnmpDataSignal kolektor2 = new SnmpDataSignal(test1);
            SnmpDataSignalQ kolektor3 = new SnmpDataSignalQ(test1);
            SnmpDataTx kolektor4 = new SnmpDataTx(test1);
            SnmpDataRx kolektor5 = new SnmpDataRx(test1);
            kolektor.Start();
            kolektor2.Start();
            kolektor3.Start();
            kolektor4.Start();
            kolektor5.Start();

            view = new LinkView(this, testLinka);
        }
    }
}
