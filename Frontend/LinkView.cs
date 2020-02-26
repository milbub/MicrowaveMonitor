using MicrowaveMonitor.Database;
using MicrowaveMonitor.Gui;
using MicrowaveMonitor.Managers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MicrowaveMonitor.Frontend
{
    internal class LinkView : Renderer
    {
        private Dictionary<int, DeviceDisplay> _devicesDisplays;
        private Link _viewedLink;
        private int _viewedDeviceId;

        public Dictionary<int, DeviceDisplay> DevicesDisplays { get => _devicesDisplays; }
        public Link ViewedLink { get => _viewedLink; }
        public int ViewedDeviceId { get => _viewedDeviceId; }

        private string tempStoreSignalData, tempStoreSignalQData, tempStoreTxData, tempStoreRxData, tempStorePingData = String.Empty;

        public LinkView(MonitoringWindow monitorGui, Link viewedLink, Dictionary<int, DeviceDisplay> deviceDisplay) : base(monitorGui)
        {
            _devicesDisplays = deviceDisplay;
            ChangeLink(viewedLink);
        }

        public void ChangeLink(Link viewedLink)
        {
            if ((ViewedLink != null) && (ViewedLink.HopCount != viewedLink.HopCount))
                ChangeDevicesConstellation();
            _viewedLink = viewedLink;
            ShowLinkName();
            if (MonitorGui.siteA.IsChecked == true)
                ChangeDevice("A");
            else
                MonitorGui.siteA.IsChecked = true;
        }

        public void ChangeDevice(string deviceLabel)
        {
            if (_viewedDeviceId != 0)
            {
                MonitorGui.ResetView();
                DevicesDisplays[ViewedDeviceId].PropertyChanged -= DataChanged;
            }

            switch (deviceLabel)
            {
                case "A":
                    _viewedDeviceId = ViewedLink.DeviceBaseId;
                    break;
                case "R1":
                    _viewedDeviceId = ViewedLink.DeviceR1Id;
                    break;
                case "R2":
                    _viewedDeviceId = ViewedLink.DeviceR2Id;
                    break;
                case "R3":
                    _viewedDeviceId = ViewedLink.DeviceR3Id;
                    break;
                case "R4":
                    _viewedDeviceId = ViewedLink.DeviceR4Id;
                    break;
                case "B":
                    _viewedDeviceId = ViewedLink.DeviceEndId;
                    break;
                default:
                    throw new NotSupportedException();
            }

            DevicesDisplays[ViewedDeviceId].PropertyChanged += DataChanged;
            
            ShowStatics();
            ShowLastData();           
            DataChanged(null, new PropertyChangedEventArgs("SysName"));
            DataChanged(null, new PropertyChangedEventArgs("Uptime"));
            DataChanged(null, new PropertyChangedEventArgs("DiffPing"));
            DataChanged(null, new PropertyChangedEventArgs("DiffSig"));
            DataChanged(null, new PropertyChangedEventArgs("DiffSigQ"));
            if (DevicesDisplays[ViewedDeviceId].DataPing != null)
                DataChanged(null, new PropertyChangedEventArgs("DataPing"));
        }

        private void DataChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "DiffPing":
                    MonitorGui.UpdateElementContent(MonitorGui.diffPing, String.Format("{0:0.0000} ms", DevicesDisplays[ViewedDeviceId].DiffPing));
                    MonitorGui.UpdateElementContent(MonitorGui.avgPing, String.Format("{0:0.00} ms", DevicesDisplays[ViewedDeviceId].AvgPing));
                    break;
                case "DiffSig":
                    MonitorGui.UpdateElementContent(MonitorGui.diffSig, String.Format("{0:0.0000} dBm", DevicesDisplays[ViewedDeviceId].DiffSig));
                    MonitorGui.UpdateElementContent(MonitorGui.avgSig, String.Format("{0:0.00} dBm", DevicesDisplays[ViewedDeviceId].AvgSig));
                    break;
                case "DiffSigQ":
                    MonitorGui.UpdateElementContent(MonitorGui.diffSigQ, String.Format("{0:0.0000} dB", DevicesDisplays[ViewedDeviceId].DiffSigQ));
                    MonitorGui.UpdateElementContent(MonitorGui.avgSigQ, String.Format("{0:0.00} dB", DevicesDisplays[ViewedDeviceId].AvgSigQ));
                    break;
                case "SysName":
                    MonitorGui.UpdateElementContent(MonitorGui.unitname, DevicesDisplays[ViewedDeviceId].SysName);
                    break;
                case "Uptime":
                    TimeSpan t = TimeSpan.FromSeconds(DevicesDisplays[ViewedDeviceId].Uptime / 100);
                    MonitorGui.UpdateElementContent(MonitorGui.uptime, String.Format("{0:D2}d {1:D2}h {2:D2}m {3:D2}s", t.Days, t.Hours, t.Minutes, t.Seconds));
                    break;
                case "DataPing":
                    MonitorGui.UpdateElementContent(MonitorGui.ping, String.Format("{0} ms", DevicesDisplays[ViewedDeviceId].DataPing.Data));
                    tempStorePingData = MonitorGui.LogWindowUpdate(MonitorGui.pingwin, MsgPing(DevicesDisplays[ViewedDeviceId].DataPing), tempStorePingData);
                    break;
                case "DataSig":
                    tempStoreSignalData = MonitorGui.LogWindowUpdate(MonitorGui.signalLevel, MsgSignal(DevicesDisplays[ViewedDeviceId].DataSig), tempStoreSignalData);
                    break;
                case "DataSigQ":
                    tempStoreSignalQData = MonitorGui.LogWindowUpdate(MonitorGui.signalQuality, MsgSignalQ(DevicesDisplays[ViewedDeviceId].DataSigQ), tempStoreSignalQData);
                    break;
                case "DataTx":
                    tempStoreTxData = MonitorGui.LogWindowUpdate(MonitorGui.tx, MsgTx(DevicesDisplays[ViewedDeviceId].DataTx), tempStoreTxData);
                    break;
                case "DataRx":
                    tempStoreRxData = MonitorGui.LogWindowUpdate(MonitorGui.rx, MsgRx(DevicesDisplays[ViewedDeviceId].DataRx), tempStoreRxData);
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }
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
            MonitorGui.UpdateElementContent(MonitorGui.linkCaption, ViewedLink.Name);
        }

        private void ShowStatics()
        {
            MonitorGui.UpdateElementContent(MonitorGui.ip, MonitorGui.GetDevice(ViewedDeviceId).Address.ToString());
        }

        private string MsgPing(Record<double> record)
        {
            string msg = String.Format("{0}      {1} ms\n", record.TimeMark.ToLongTimeString(), record.Data.ToString());
            return msg;
        }

        private string MsgSignal(Record<double> record)
        {
            string msg = String.Format("{0}      {1} dBm\n", record.TimeMark.ToLongTimeString(), record.Data.ToString());
            return msg;
        }

        private string MsgSignalQ(Record<double> record)
        {
            string msg = String.Format("{0}      {1} dB\n", record.TimeMark.ToLongTimeString(), record.Data.ToString());
            return msg;
        }

        private string MsgTx(Record<uint> record)
        {
            double dataRate = record.Data / 1000;
            string msg = String.Format("{0}      {1} kbit/s\n", record.TimeMark.ToLongTimeString(), dataRate.ToString());
            return msg;
        }

        private string MsgRx(Record <uint> record)
        {
            double dataRate = record.Data / 1000;
            string msg = String.Format("{0}      {1} kbit/s\n", record.TimeMark.ToLongTimeString(), dataRate.ToString());
            return msg;
        }

        private void ShowLastData()
        {
            //string msgS = String.Empty;
            //string msgSQ = String.Empty;
            //string msgTx = String.Empty;
            //string msgRx = String.Empty;
            //string msgPg = String.Empty;

            //int countS = ViewedDevice.DataSignal.Count;
            //int countSQ = ViewedDevice.DataSignalQ.Count;
            //int countTx = ViewedDevice.DataTx.Count;
            //int countRx = ViewedDevice.DataRx.Count;
            //int countPg = ViewedDevice.DataPing.Count;
            //int position;

            //position = LastDataPosition(countS);
            //for (; position < countS; position++)
            //{
            //    msgS += MsgSignal(position);
            //}

            //position = LastDataPosition(countSQ);
            //for (; position < countSQ; position++)
            //{
            //    msgSQ += MsgSignalQ(position);
            //}

            //position = LastDataPosition(countTx);
            //for (; position < countTx; position++)
            //{
            //    msgTx += MsgTx(position);
            //}

            //position = LastDataPosition(countRx);
            //for (; position < countRx; position++)
            //{
            //    msgRx += MsgRx(position);
            //}

            //position = LastDataPosition(countPg);
            //for (; position < countPg; position++)
            //{
            //    msgPg += MsgPing(position);
            //}

            //tempStoreSignalData = MonitorGui.LogWindowUpdate(MonitorGui.signalLevel, msgS, tempStoreSignalData);
            //tempStoreSignalQData = MonitorGui.LogWindowUpdate(MonitorGui.signalQuality, msgSQ, tempStoreSignalQData);
            //tempStoreTxData = MonitorGui.LogWindowUpdate(MonitorGui.tx, msgTx, tempStoreTxData);
            //tempStoreRxData = MonitorGui.LogWindowUpdate(MonitorGui.rx, msgRx, tempStoreRxData);
            //tempStorePingData = MonitorGui.LogWindowUpdate(MonitorGui.pingwin, msgPg, tempStorePingData);
        }

        //private int LastDataPosition(int count)
        //{
        //    int position;
        //    if (count > 50)
        //        position = count - 50;
        //    else
        //        position = 0;
        //    return position;
        //}
    }
}
