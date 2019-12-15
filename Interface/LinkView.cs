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

        private string tempStoreSignalData, tempStoreSignalQData, tempStoreTxData, tempStoreRxData = String.Empty;

        public LinkView(MainWindow monitorGui, Link monitoredLink)
        {
            MonitorGui = monitorGui;
            MonitoredLink = monitoredLink;

            SetLinkName();
            StartUptimeUpdater();
            StartLogWindowCleaner();          

            MonitoredLink.BaseDevice.DataSignal.CollectionChanged += BaseSignalDataChanged;
            MonitoredLink.BaseDevice.DataSignalQ.CollectionChanged += BaseSignalQDataChanged;
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

        private string LogWindowUpdate(System.Windows.Controls.TextBox logWindow, string newMessage, string tempMessage)
        {
            if (!logWindow.Dispatcher.CheckAccess())
            {
                logWindow.Dispatcher.Invoke(() =>
                {
                    if (!logWindow.IsFocused)
                    {
                        logWindow.Text += tempMessage + newMessage;
                        tempMessage = String.Empty;
                        logWindow.ScrollToEnd();
                    }
                    else
                    {
                        tempMessage += newMessage;
                    }
                });
            }
            else
            {
                if (!logWindow.IsFocused)
                {
                    logWindow.Text += tempMessage + newMessage;
                    tempMessage = String.Empty;
                    logWindow.ScrollToEnd();
                }
                else
                {
                    tempMessage += newMessage;
                }
            }

            return tempMessage;
        }

        private void BaseSignalDataChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            string msg = String.Format("{0}    {1} dBm\n", MonitoredLink.BaseDevice.DataSignal.Last().TimeMark.ToLongTimeString(), MonitoredLink.BaseDevice.DataSignal.Last().Data.ToString());
            tempStoreSignalData = LogWindowUpdate(MonitorGui.signalLevel, msg, tempStoreSignalData);
        }

        private void BaseSignalQDataChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            string msg = String.Format("{0}    {1} dB\n", MonitoredLink.BaseDevice.DataSignalQ.Last().TimeMark.ToLongTimeString(), MonitoredLink.BaseDevice.DataSignalQ.Last().Data.ToString());
            tempStoreSignalQData = LogWindowUpdate(MonitorGui.signalQuality, msg, tempStoreSignalQData);
        }

        private void BaseTxDataChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            double dataRate = MonitoredLink.BaseDevice.DataTx.Last().Data / 1000;
            string msg = String.Format("{0}    {1} kbit/s\n", MonitoredLink.BaseDevice.DataTx.Last().TimeMark.ToLongTimeString(), dataRate.ToString());
            tempStoreTxData = LogWindowUpdate(MonitorGui.tx, msg, tempStoreTxData);
        }

        private void BaseRxDataChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            double dataRate = MonitoredLink.BaseDevice.DataRx.Last().Data / 1000;
            string msg = String.Format("{0}    {1} kbit/s\n", MonitoredLink.BaseDevice.DataRx.Last().TimeMark.ToLongTimeString(), dataRate.ToString());
            tempStoreRxData = LogWindowUpdate(MonitorGui.rx, msg, tempStoreRxData);
        }

        private void LogWindowCleaner(System.Windows.Controls.TextBox logWindow, int permittedLinesCount)
        {
            if (!logWindow.Dispatcher.CheckAccess())
            {
                logWindow.Dispatcher.Invoke(() =>
                {
                    var splitted = logWindow.Text.Split('\n');
                    int linesCount = splitted.Length;
                    if (linesCount > permittedLinesCount)
                    {
                        logWindow.Text = String.Join("\n", splitted.Skip(linesCount - permittedLinesCount));
                    }
                });
            }
            else
            {
                var splitted = logWindow.Text.Split('\n');
                int linesCount = splitted.Length;
                if (linesCount > permittedLinesCount)
                {
                    logWindow.Text = String.Join("\n", splitted.Skip(linesCount - permittedLinesCount));
                }
            }
        }

        private void StartLogWindowCleaner()
        {
            Task.Run(() =>
            {
                Thread.Sleep(10000);
                while (true)
                {
                    LogWindowCleaner(MonitorGui.signalLevel, 50);
                    LogWindowCleaner(MonitorGui.signalQuality, 50);
                    LogWindowCleaner(MonitorGui.tx, 50);
                    LogWindowCleaner(MonitorGui.rx, 50);
                    Thread.Sleep(10000);
                }
            });
        }
    }
}
