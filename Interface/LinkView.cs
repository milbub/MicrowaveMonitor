﻿using MicrowaveMonitor.Database;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MicrowaveMonitor.Interface
{
    internal class LinkView
    {
        private MonitoringWindow _monitorGui;
        private Link _viewedLink;
        private Device _viewedDevice;

        public MonitoringWindow MonitorGui { get => _monitorGui; }
        public Link ViewedLink { get => _viewedLink; set => _viewedLink = value; }
        public Device ViewedDevice { get => _viewedDevice; }

        private string tempStoreSignalData, tempStoreSignalQData, tempStoreTxData, tempStoreRxData, tempStorePingData = String.Empty;

        public LinkView(MonitoringWindow monitorGui, Link viewedLink)
        {
            _monitorGui = monitorGui;
            ChangeLink(viewedLink);
        }

        public void ChangeLink(Link viewedLink)
        {
            if ((ViewedLink != null) && (ViewedLink.HopCount != viewedLink.HopCount))
                ChangeDevicesConstellation();
            ViewedLink = viewedLink;
            ShowLinkName();
            if (MonitorGui.siteA.IsChecked == true)
                ChangeDevice("A");
            else
                MonitorGui.siteA.IsChecked = true;
        }

        public void ChangeDevice(string deviceLabel)
        {
            if (_viewedDevice != null)
            {
                MonitorGui.ResetView();
                UnregisterCharts();
            }

            switch (deviceLabel)
            {
                case "A":
                    _viewedDevice = ViewedLink.BaseDevice;
                    break;
                case "R1":
                    _viewedDevice = ViewedLink.RelayOne;
                    break;
                case "R2":
                    _viewedDevice = ViewedLink.RelayTwo;
                    break;
                case "R3":
                    _viewedDevice = ViewedLink.RelayThree;
                    break;
                case "R4":
                    _viewedDevice = ViewedLink.RelayFour;
                    break;
                case "B":
                    _viewedDevice = ViewedLink.EndDevice;
                    break;
                default:
                    throw new ArgumentException();
            }

            RegisterCharts();
            ShowIp();
            ShowLastData();
            
            try
            {
                StaticsChanged(_viewedDevice, new PropertyChangedEventArgs("sysName"));
                StaticsChanged(_viewedDevice, new PropertyChangedEventArgs("uptime"));
                ShowPing(String.Format("{0} ms", _viewedDevice.DataPing.Last().Data));
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }
            
            updateElement(MonitorGui.avgSig, String.Format("{0:0.00} dBm", ViewedDevice.AvgSig));
            updateElement(MonitorGui.diffSig, String.Format("{0:0.0000} dBm", ViewedDevice.DiffSig));
            updateElement(MonitorGui.avgSigQ, String.Format("{0:0.00} dB", ViewedDevice.AvgSigQ));
            updateElement(MonitorGui.diffSigQ, String.Format("{0:0.0000} dB", ViewedDevice.DiffSigQ));
            updateElement(MonitorGui.avgPing, String.Format("{0:0.00} ms", ViewedDevice.AvgPing));
            updateElement(MonitorGui.diffPing, String.Format("{0:0.0000} ms", ViewedDevice.DiffPing));
        }

        private void UnregisterCharts()
        {
            _viewedDevice.DataSignal.CollectionChanged -= SignalDataChanged;
            _viewedDevice.DataSignalQ.CollectionChanged -= SignalQDataChanged;
            _viewedDevice.DataTx.CollectionChanged -= TxDataChanged;
            _viewedDevice.DataRx.CollectionChanged -= RxDataChanged;
            _viewedDevice.DataPing.CollectionChanged -= PingDataChanged;
            _viewedDevice.PropertyChanged -= StaticsChanged;
        }

        private void RegisterCharts()
        {
            _viewedDevice.DataSignal.CollectionChanged += SignalDataChanged;
            _viewedDevice.DataSignalQ.CollectionChanged += SignalQDataChanged;
            _viewedDevice.DataTx.CollectionChanged += TxDataChanged;
            _viewedDevice.DataRx.CollectionChanged += RxDataChanged;
            _viewedDevice.DataPing.CollectionChanged += PingDataChanged;
            _viewedDevice.PropertyChanged += StaticsChanged;
        }

        public void ChangeDevicesConstellation()
        {
            ConstellationChanger(false, 5);
            ConstellationChanger(true, ViewedLink.HopCount);
        }

        private void ConstellationChanger(bool state, byte selector)
        {
            switch (selector)
            {
                case 0:
                    MonitorGui.SiteChooserEnabler(state, MonitorGui.siteA);
                    break;
                case 1:
                    MonitorGui.SiteChooserEnabler(state, MonitorGui.siteB);
                    goto case 0;
                case 2:
                    MonitorGui.SiteChooserEnabler(state, MonitorGui.siteR1);
                    goto case 1;
                case 3:
                    MonitorGui.SiteChooserEnabler(state, MonitorGui.siteR2);
                    goto case 2;
                case 4:
                    MonitorGui.SiteChooserEnabler(state, MonitorGui.siteR3);
                    goto case 3;
                case 5:
                    MonitorGui.SiteChooserEnabler(state, MonitorGui.siteR4);
                    goto case 4;
                default:
                    throw new NotSupportedException();
            }
        }

        private void ShowLinkName()
        {
            updateElement(MonitorGui.linkCaption, ViewedLink.Name);
        }

        private void ShowIp()
        {
            updateElement(MonitorGui.ip, ViewedDevice.Address.Address.ToString());
        }

        private void ShowPing(string pingValue)
        {
            updateElement(MonitorGui.ping, pingValue);
        }

        private void StaticsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "sysName")
            {
                updateElement(MonitorGui.unitname, ViewedDevice.DataSysName);
            }
            else if (e.PropertyName == "uptime")
            {
                TimeSpan t = TimeSpan.FromSeconds(ViewedDevice.DataUptime / 100);
                updateElement(MonitorGui.uptime, String.Format("{0:D2}d {1:D2}h {2:D2}m {3:D2}s", t.Days, t.Hours, t.Minutes, t.Seconds));
            }
        }

        private void updateElement(System.Windows.Controls.ContentControl element, string value)
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

        private void SignalDataChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            tempStoreSignalData = LogWindowUpdate(MonitorGui.signalLevel, MsgSignal(e.NewStartingIndex), tempStoreSignalData);
            updateElement(MonitorGui.avgSig, String.Format("{0:0.00} dBm", ViewedDevice.AvgSig));
            updateElement(MonitorGui.diffSig, String.Format("{0:0.0000} dBm", ViewedDevice.DiffSig));
        }

        private string MsgSignal(int position)
        {
            string msg = String.Format("{0}      {1} dBm\n", ViewedDevice.DataSignal.ElementAt(position).TimeMark.ToLongTimeString(), ViewedDevice.DataSignal.ElementAt(position).Data.ToString());
            return msg;
        }

        private void SignalQDataChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            tempStoreSignalQData = LogWindowUpdate(MonitorGui.signalQuality, MsgSignalQ(e.NewStartingIndex), tempStoreSignalQData);
            updateElement(MonitorGui.avgSigQ, String.Format("{0:0.00} dB", ViewedDevice.AvgSigQ));
            updateElement(MonitorGui.diffSigQ, String.Format("{0:0.0000} dB", ViewedDevice.DiffSigQ));
        }

        private string MsgSignalQ(int position)
        {
            string msg = String.Format("{0}      {1} dB\n", ViewedDevice.DataSignalQ.ElementAt(position).TimeMark.ToLongTimeString(), ViewedDevice.DataSignalQ.ElementAt(position).Data.ToString());
            return msg;
        }

        private void TxDataChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            tempStoreTxData = LogWindowUpdate(MonitorGui.tx, MsgTx(e.NewStartingIndex), tempStoreTxData);
        }

        private string MsgTx(int position)
        {
            double dataRate = ViewedDevice.DataTx.ElementAt(position).Data / 1000;
            string msg = String.Format("{0}      {1} kbit/s\n", ViewedDevice.DataTx.ElementAt(position).TimeMark.ToLongTimeString(), dataRate.ToString());
            return msg;
        }

        private void RxDataChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            tempStoreRxData = LogWindowUpdate(MonitorGui.rx, MsgRx(e.NewStartingIndex), tempStoreRxData);
        }

        private string MsgRx(int position)
        {
            double dataRate = ViewedDevice.DataRx.ElementAt(position).Data / 1000;
            string msg = String.Format("{0}      {1} kbit/s\n", ViewedDevice.DataRx.ElementAt(position).TimeMark.ToLongTimeString(), dataRate.ToString());
            return msg;
        }

        private void PingDataChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            string msg = MsgPing(e.NewStartingIndex);
            tempStorePingData = LogWindowUpdate(MonitorGui.pingwin, msg, tempStorePingData);
            ShowPing(String.Format("{0} ms", _viewedDevice.DataPing.ElementAt(e.NewStartingIndex).Data));
            updateElement(MonitorGui.avgPing, String.Format("{0:0.00} ms", ViewedDevice.AvgPing));
            updateElement(MonitorGui.diffPing, String.Format("{0:0.0000} ms", ViewedDevice.DiffPing));
        }

        private string MsgPing(int position)
        {
            string msg = String.Format("{0}      {1} ms\n", ViewedDevice.DataPing.ElementAt(position).TimeMark.ToLongTimeString(), ViewedDevice.DataPing.ElementAt(position).Data.ToString());
            return msg;
        }

        private void ShowLastData()
        {
            string msgS = String.Empty;
            string msgSQ = String.Empty;
            string msgTx = String.Empty;
            string msgRx = String.Empty;
            string msgPg = String.Empty;

            int countS = ViewedDevice.DataSignal.Count;
            int countSQ = ViewedDevice.DataSignalQ.Count;
            int countTx = ViewedDevice.DataTx.Count;
            int countRx = ViewedDevice.DataRx.Count;
            int countPg = ViewedDevice.DataPing.Count;
            int position;

            position = LastDataPosition(countS);
            for (; position < countS; position++)
            {
                msgS += MsgSignal(position);
            }

            position = LastDataPosition(countSQ);
            for (; position < countSQ; position++)
            {
                msgSQ += MsgSignalQ(position);
            }

            position = LastDataPosition(countTx);
            for (; position < countTx; position++)
            {
                msgTx += MsgTx(position);
            }

            position = LastDataPosition(countRx);
            for (; position < countRx; position++)
            {
                msgRx += MsgRx(position);
            }

            position = LastDataPosition(countPg);
            for (; position < countPg; position++)
            {
                msgPg += MsgPing(position);
            }

            tempStoreSignalData = LogWindowUpdate(MonitorGui.signalLevel, msgS, tempStoreRxData);
            tempStoreSignalQData = LogWindowUpdate(MonitorGui.signalQuality, msgSQ, tempStoreRxData);
            tempStoreTxData = LogWindowUpdate(MonitorGui.tx, msgTx, tempStoreRxData);
            tempStoreRxData = LogWindowUpdate(MonitorGui.rx, msgRx, tempStoreRxData);
            tempStorePingData = LogWindowUpdate(MonitorGui.pingwin, msgPg, tempStorePingData);
        }

        private int LastDataPosition(int count)
        {
            int position;
            if (count > 50)
                position = count - 50;
            else
                position = 0;
            return position;
        }

        private string LogWindowUpdate(System.Windows.Controls.TextBox logWindow, string newMessage, string tempMessage)
        {
            try
            {
                if (!logWindow.Dispatcher.CheckAccess())
                {
                    logWindow.Dispatcher.Invoke(() =>
                    {
                        if (logWindow.IsSelectionActive)
                        {
                            tempMessage += newMessage;
                        }
                        else
                        {
                            logWindow.Text += tempMessage + newMessage;
                            tempMessage = String.Empty;
                            logWindow.ScrollToEnd();
                        }
                    });
                }
                else
                {
                    if (logWindow.IsSelectionActive)
                    {
                        tempMessage += newMessage;
                    }
                    else
                    {
                        logWindow.Text += tempMessage + newMessage;
                        tempMessage = String.Empty;
                        logWindow.ScrollToEnd();
                    }
                }
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine(e.Message);
            }

            LogWindowCleaner(logWindow, 50);
            return tempMessage;
        }

        private void LogWindowCleaner(System.Windows.Controls.TextBox logWindow, int permittedLinesCount)
        {
            try
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
            catch (TaskCanceledException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
