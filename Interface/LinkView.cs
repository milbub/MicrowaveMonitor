using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using MicrowaveMonitor.Database;

namespace MicrowaveMonitor.Interface
{
    class LinkView
    {
        MainWindow _monitorGui;
        Link _monitoredLink;

        public MainWindow MonitorGui { get => _monitorGui; set => _monitorGui = value; }
        public Link MonitoredLink { get => _monitoredLink; set => _monitoredLink = value; }

        public LinkView(MainWindow monitorGui, Link monitoredLink)
        {
            MonitorGui = monitorGui;
            MonitoredLink = monitoredLink;

            SetLinkName();
            StartUptimeUpdater();

            MonitoredLink.BaseDevice.DataSignalLevel.CollectionChanged += BaseSignalDataChanged;
            MonitoredLink.BaseDevice.DataSignalQuality.CollectionChanged += BaseSignalQDataChanged;
            MonitoredLink.BaseDevice.DataTx.CollectionChanged += BaseTxDataChanged;
            MonitoredLink.BaseDevice.DataRx.CollectionChanged += BaseRxDataChanged;
        }

        private void SetLinkName()
        {
            if (!MonitorGui.linkCaption.Dispatcher.CheckAccess())
            {
                MonitorGui.linkCaption.Dispatcher.Invoke(() =>
                {
                    MonitorGui.linkCaption.Content = MonitoredLink.Name;
                });
            }
            else
            {
                MonitorGui.linkCaption.Content = MonitoredLink.Name;
            }
        }

        private void SetUptime()
        {
            if (!MonitorGui.uptime.Dispatcher.CheckAccess())
            {
                MonitorGui.uptime.Dispatcher.Invoke(() =>
                {
                    MonitorGui.uptime.Content = MonitoredLink.BaseDevice.DataUptime;
                });
            }
            else
            {
                MonitorGui.uptime.Content = MonitoredLink.BaseDevice.DataUptime;
            }
        }

        private void StartUptimeUpdater()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    SetUptime();
                    Thread.Sleep(10000);
                }
            });
        }

        private void BaseSignalDataChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            string msg = String.Format("{0}    {1} dBm\n", MonitoredLink.BaseDevice.DataSignalLevel.Last().TimeMark.ToLongTimeString(), MonitoredLink.BaseDevice.DataSignalLevel.Last().Data.ToString());

            if (!MonitorGui.signalLevel.Dispatcher.CheckAccess())
            {
                MonitorGui.signalLevel.Dispatcher.Invoke(() =>
                {
                    MonitorGui.signalLevel.Text += msg;
                    MonitorGui.signalLevel.ScrollToEnd();
                });
            }
            else
            {
                MonitorGui.signalLevel.Text += msg;
                MonitorGui.signalLevel.ScrollToEnd();
            }
        }

        private void BaseSignalQDataChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            string msg = String.Format("{0}    {1} dB\n", MonitoredLink.BaseDevice.DataSignalQuality.Last().TimeMark.ToLongTimeString(), MonitoredLink.BaseDevice.DataSignalQuality.Last().Data.ToString());

            if (!MonitorGui.signalQuality.Dispatcher.CheckAccess())
            {
                MonitorGui.signalQuality.Dispatcher.Invoke(() =>
                {
                    MonitorGui.signalQuality.Text += msg;
                    MonitorGui.signalQuality.ScrollToEnd();
                });
            }
            else
            {
                MonitorGui.signalQuality.Text += msg;
                MonitorGui.signalQuality.ScrollToEnd();
            }
        }

        private void BaseTxDataChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            double dataRate = MonitoredLink.BaseDevice.DataTx.Last().Data / 1000;
            string msg = String.Format("{0}    {1} kbit/s\n", MonitoredLink.BaseDevice.DataTx.Last().TimeMark.ToLongTimeString(), dataRate.ToString());

            if (!MonitorGui.tx.Dispatcher.CheckAccess())
            {
                MonitorGui.tx.Dispatcher.Invoke(() =>
                {
                    MonitorGui.tx.Text += msg;
                    MonitorGui.tx.ScrollToEnd();
                });
            }
            else
            {
                MonitorGui.tx.Text += msg;
                MonitorGui.tx.ScrollToEnd();
            }
        }

        private void BaseRxDataChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            double dataRate = MonitoredLink.BaseDevice.DataRx.Last().Data / 1000;
            string msg = String.Format("{0}    {1} kbit/s\n", MonitoredLink.BaseDevice.DataRx.Last().TimeMark.ToLongTimeString(), dataRate.ToString());

            if (!MonitorGui.rx.Dispatcher.CheckAccess())
            {
                MonitorGui.rx.Dispatcher.Invoke(() =>
                {
                    MonitorGui.rx.Text += msg;
                    MonitorGui.rx.ScrollToEnd();
                });
            }
            else
            {
                MonitorGui.rx.Text += msg;
                MonitorGui.rx.ScrollToEnd();
            }
        }
    }
}
