using MicrowaveMonitor.Database;
using MicrowaveMonitor.Gui;
using MicrowaveMonitor.Managers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;

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
            MonitorGui.UpdateElementContent(MonitorGui.linkCaption, ViewedLink.Name);
            
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
                    MonitorGui.GraphUpdate(MonitorGui.pingwin, DevicesDisplays[ViewedDeviceId].DataPing);
                    break;
                case "DataSig":
                    MonitorGui.GraphUpdate(MonitorGui.signalLevel, DevicesDisplays[ViewedDeviceId].DataSig);
                    break;
                case "DataSigQ":
                    MonitorGui.GraphUpdate(MonitorGui.signalQuality, DevicesDisplays[ViewedDeviceId].DataSigQ);
                    break;
                case "DataTx":
                    var convertedTx = new Record<double>(DevicesDisplays[ViewedDeviceId].DataTx.TimeMark, DevicesDisplays[ViewedDeviceId].DataTx.Data);
                    MonitorGui.GraphUpdate(MonitorGui.tx, convertedTx);
                    break;
                case "DataRx":
                    var convertedRx = new Record<double>(DevicesDisplays[ViewedDeviceId].DataRx.TimeMark, DevicesDisplays[ViewedDeviceId].DataRx.Data);
                    MonitorGui.GraphUpdate(MonitorGui.rx, convertedRx);
                    break;
                case "DataTempOdu":
                    MonitorGui.GraphUpdate(MonitorGui.tempOdu, DevicesDisplays[ViewedDeviceId].DataTempOdu);
                    break;
                case "DataTempIdu":
                    MonitorGui.GraphUpdate(MonitorGui.tempIdu, DevicesDisplays[ViewedDeviceId].DataTempIdu);
                    break;
                case "DataVoltage":
                    MonitorGui.GraphUpdate(MonitorGui.voltage, DevicesDisplays[ViewedDeviceId].DataVoltage);
                    break;
                case "WeatherIcon":
                    System.Windows.Media.Imaging.BitmapImage bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(String.Format(ConfigurationManager.AppSettings.Get("WeatherApiIconSource"), DevicesDisplays[ViewedDeviceId].WeatherIcon));
                    bitmapImage.EndInit();
                    MonitorGui.UpdateImage(MonitorGui.weatherIcon, bitmapImage);
                    break;
                case "WeatherDesc":
                    MonitorGui.UpdateElementContent(MonitorGui.weatherDesc, DevicesDisplays[ViewedDeviceId].WeatherDesc.ToString());
                    break;
                case "WeatherTemp":
                    MonitorGui.UpdateElementContent(MonitorGui.weatherTemp, DevicesDisplays[ViewedDeviceId].WeatherTemp.ToString() + " °C");
                    break;
                case "WeatherWind":
                    MonitorGui.UpdateElementContent(MonitorGui.weatherWind, DevicesDisplays[ViewedDeviceId].WeatherWind.ToString() + " m/s");
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

        private void ShowStatics()
        {
            MonitorGui.UpdateElementContent(MonitorGui.ip, MonitorGui.GetDevice(ViewedDeviceId).Address.ToString());
            DataChanged(null, new PropertyChangedEventArgs("SysName"));
            DataChanged(null, new PropertyChangedEventArgs("Uptime"));
            DataChanged(null, new PropertyChangedEventArgs("DiffPing"));
            DataChanged(null, new PropertyChangedEventArgs("DiffSig"));
            DataChanged(null, new PropertyChangedEventArgs("DiffSigQ"));
            DataChanged(null, new PropertyChangedEventArgs("WeatherIcon"));
            DataChanged(null, new PropertyChangedEventArgs("WeatherDesc"));
            DataChanged(null, new PropertyChangedEventArgs("WeatherTemp"));
            DataChanged(null, new PropertyChangedEventArgs("WeatherWind"));
            if (DevicesDisplays[ViewedDeviceId].DataPing != null)
                DataChanged(null, new PropertyChangedEventArgs("DataPing"));
        }
    }
}
