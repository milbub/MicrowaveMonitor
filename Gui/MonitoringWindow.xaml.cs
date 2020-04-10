using MicrowaveMonitor.Database;
using MicrowaveMonitor.Gui;
using MicrowaveMonitor.Properties;
using MicrowaveMonitor.Managers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;

namespace MicrowaveMonitor.Gui
{
    public partial class MonitoringWindow : Window
    {
        public enum DeviceType { Base, End, R1, R2, R3, R4 };

        private readonly LinkManager linkM;
        private readonly WorkerManager workerM;
        private readonly AlarmManager alarmM;
        private readonly DataManager dataM;

        private readonly Dictionary<int, DeviceDisplay> devicesDisplays;
        private Link viewedLink;
        private int viewedDeviceId = 0;

        public MonitoringWindow(LinkManager linkManager, WorkerManager workerManager, AlarmManager alarmManager, DataManager dataManager)
        {
            linkM = linkManager;
            workerM = workerManager;
            alarmM = alarmManager;
            dataM = dataManager;
            devicesDisplays = workerManager.DeviceToFront;

            InitializeComponent();
            RegisterCharts();
            ChangeLink(linkManager.LinkDatabase.Get<Link>(linkManager.LinkNames.First().Key));

            LinksList.ItemsSource = linkManager.LinkNames.Values;
            LinksList.SelectedItem = linkManager.LinkNames.First().Value;
            LinksList.SelectionChanged += LinkChoosed;

            alarmListPane.AlarmsList.ItemsSource = alarmManager.Alarms;

            siteA.Checked += SiteChoosed;
            siteB.Checked += SiteChoosed;
            siteR1.Checked += SiteChoosed;
            siteR2.Checked += SiteChoosed;
            siteR3.Checked += SiteChoosed;
            siteR4.Checked += SiteChoosed;

            tx.chart.axisY.MinValue = 0;
            rx.chart.axisY.MinValue = 0;
            voltage.chart.axisY.MinValue = 0;
            latency.chart.axisY.MinValue = 0;
        }

        public void ChangeLink(Link viewedLink)
        {
            this.viewedLink = viewedLink;
            linkCaption.Content = viewedLink.Name;
            FillSettings();

            if (siteA.IsChecked == true)
                ChangeDevice("A");
            else
                siteA.IsChecked = true;
        }

        public void ChangeDevice(string deviceLabel)
        {
            if (viewedDeviceId != 0)
            {
                ResetView();
                devicesDisplays[viewedDeviceId].PropertyChanged -= DataChangedDispatch;
            }

            switch (deviceLabel)
            {
                case "A":
                    viewedDeviceId = viewedLink.DeviceBaseId;
                    break;
                case "R1":
                    viewedDeviceId = viewedLink.DeviceR1Id;
                    break;
                case "R2":
                    viewedDeviceId = viewedLink.DeviceR2Id;
                    break;
                case "R3":
                    viewedDeviceId = viewedLink.DeviceR3Id;
                    break;
                case "R4":
                    viewedDeviceId = viewedLink.DeviceR4Id;
                    break;
                case "B":
                    viewedDeviceId = viewedLink.DeviceEndId;
                    break;
                default:
                    throw new NotSupportedException();
            }

            Device device = linkM.GetDevice(viewedDeviceId);
            ChartsChangeDevice(viewedDeviceId, device);

            ContextChanged();
            ShowStatics(device);

            devicesDisplays[viewedDeviceId].PropertyChanged += DataChangedDispatch;
        }

        private void ContextChanged()
        {
            signal.HistoryChanged(null, null);
            signalQ.HistoryChanged(null, null);
            tempOdu.HistoryChanged(null, null);
            tempIdu.HistoryChanged(null, null);
            voltage.HistoryChanged(null, null);
            tx.HistoryChanged(null, null);
            rx.HistoryChanged(null, null);
            latency.HistoryChanged(null, null);
        }

        private void ShowStatics(Device device)
        {
            ip.Content = device.Address.ToString();
            DataChanged(devicesDisplays[viewedDeviceId], new PropertyChangedEventArgs("SysName"));
            DataChanged(devicesDisplays[viewedDeviceId], new PropertyChangedEventArgs("State"));
            DataChanged(devicesDisplays[viewedDeviceId], new PropertyChangedEventArgs("Uptime"));
            DataChanged(devicesDisplays[viewedDeviceId], new PropertyChangedEventArgs("WeatherIcon"));
            DataChanged(devicesDisplays[viewedDeviceId], new PropertyChangedEventArgs("WeatherDesc"));
            DataChanged(devicesDisplays[viewedDeviceId], new PropertyChangedEventArgs("WeatherTemp"));
            DataChanged(devicesDisplays[viewedDeviceId], new PropertyChangedEventArgs("WeatherWind"));
            if (devicesDisplays[viewedDeviceId].DataPing != null)
                DataChanged(devicesDisplays[viewedDeviceId], new PropertyChangedEventArgs("DataPing"));
        }

        private void FillSettings()
        {
            boxLinkName.Text = viewedLink.Name;
            boxNote.Text = viewedLink.Note;

            settingsA.FillBoxes(linkM, viewedLink.DeviceBaseId);
            siteB.IsEnabled = viewedLink.DeviceEndId > 0 ? settingsB.FillBoxes(linkM, viewedLink.DeviceEndId) : false;
            checkB.IsChecked = viewedLink.DeviceEndId > 0 ? settingsB.FillBoxes(linkM, viewedLink.DeviceEndId) : false;
            siteR1.IsEnabled = viewedLink.DeviceR1Id > 0 ? settingsR1.FillBoxes(linkM, viewedLink.DeviceR1Id) : false;
            checkR1.IsChecked = viewedLink.DeviceR1Id > 0 ? settingsR1.FillBoxes(linkM, viewedLink.DeviceR1Id) : false;
            siteR2.IsEnabled = viewedLink.DeviceR2Id > 0 ? settingsR2.FillBoxes(linkM, viewedLink.DeviceR2Id) : false;
            checkR2.IsChecked = viewedLink.DeviceR2Id > 0 ? settingsR2.FillBoxes(linkM, viewedLink.DeviceR2Id) : false;
            siteR3.IsEnabled = viewedLink.DeviceR3Id > 0 ? settingsR3.FillBoxes(linkM, viewedLink.DeviceR3Id) : false;
            checkR3.IsChecked = viewedLink.DeviceR3Id > 0 ? settingsR3.FillBoxes(linkM, viewedLink.DeviceR3Id) : false;
            siteR4.IsEnabled = viewedLink.DeviceR4Id > 0 ? settingsR4.FillBoxes(linkM, viewedLink.DeviceR4Id) : false;
            checkR4.IsChecked = viewedLink.DeviceR4Id > 0 ? settingsR4.FillBoxes(linkM, viewedLink.DeviceR4Id) : false;
        }

        private void DataChangedDispatch(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(() =>
                    {
                        DataChanged(sender, e);
                    });
                }
                else
                {
                    DataChanged(sender, e);
                }
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async void DataChanged(object sender, PropertyChangedEventArgs e)
        {
            DeviceDisplay display = (DeviceDisplay)sender;
            
            if (display == devicesDisplays[viewedDeviceId])
                switch (e.PropertyName)
                {
                    case "SysName":
                        unitname.Content = display.SysName;
                        break;
                    case "State":
                        ChangeLinkState(display.State);
                        break;
                    case "Uptime":
                        TimeSpan t = TimeSpan.FromSeconds(display.Uptime / 100);
                        uptime.Content = String.Format("{0:D2}d {1:D2}h {2:D2}m {3:D2}s", t.Days, t.Hours, t.Minutes, t.Seconds);
                        break;
                    case "DataPing":
                        ping.Content = String.Format("{0} ms", display.DataPing.Data);
                        await latency.GraphUpdate(display.DataPing, null);
                        break;
                    case "DataSig":
                        await signal.GraphUpdate(display.DataSig, null);
                        break;
                    case "DataSigQ":
                        await signalQ.GraphUpdate(display.DataSigQ, null);
                        break;
                    case "DataTx":
                        var convertedTx = new Record<double>(display.DataTx.TimeMark, display.DataTx.Data);
                        await tx.GraphUpdate(convertedTx, null);
                        break;
                    case "DataRx":
                        var convertedRx = new Record<double>(display.DataRx.TimeMark, display.DataRx.Data);
                        await rx.GraphUpdate(convertedRx, null);
                        break;
                    case "DataTempOdu":
                        await tempOdu.GraphUpdate(display.DataTempOdu, null);
                        break;
                    case "DataTempIdu":
                        await tempIdu.GraphUpdate(display.DataTempIdu, null);
                        break;
                    case "DataVoltage":
                        await voltage.GraphUpdate(display.DataVoltage, null);
                        break;
                    case "WeatherIcon":
                        Bitmap iconBitmap = (Bitmap)Properties.Resources.ResourceManager.GetObject(display.WeatherIcon);
                        weatherIcon.Source = BitmapToImageSource(iconBitmap);
                        break;
                    case "WeatherDesc":
                        weatherDesc.Content = display.WeatherDesc.ToString();
                        break;
                    case "WeatherTemp":
                        weatherTemp.Content = String.Format("{0:0} °C", display.WeatherTemp);
                        break;
                    case "WeatherWind":
                        weatherWind.Content = display.WeatherWind.ToString() + " m/s";
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }
        }

        private void ChangeLinkState(DeviceDisplay.LinkState state)
        {
            switch (state)
            {
                case DeviceDisplay.LinkState.Paused:
                    linkState.Content = "paused";
                    linkState.Foreground = System.Windows.Media.Brushes.DarkSlateGray;
                    buttonPause.IsEnabled = false;
                    buttonStart.IsEnabled = true;
                    break;
                case DeviceDisplay.LinkState.Running:
                    linkState.Content = "running";
                    linkState.Foreground = System.Windows.Media.Brushes.Green;
                    buttonStart.IsEnabled = false;
                    buttonPause.IsEnabled = true;
                    break;
                case DeviceDisplay.LinkState.AlarmWarning:
                    linkState.Content = "warning";
                    linkState.Foreground = System.Windows.Media.Brushes.Gold;
                    break;
                case DeviceDisplay.LinkState.AlarmCritical:
                    linkState.Content = "critical";
                    linkState.Foreground = System.Windows.Media.Brushes.Crimson;
                    break;
                case DeviceDisplay.LinkState.AlarmDown:
                    linkState.Content = "down";
                    linkState.Foreground = System.Windows.Media.Brushes.DarkRed;
                    break;
                default:
                    break;
            }
        }

        private void SiteChoosed(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb.IsChecked.Value)
                ChangeDevice(rb.Content.ToString());
        }

        private void LinkChoosed(object sender, SelectionChangedEventArgs e)
        {
            ChangeLink(linkM.LinkDatabase.Get<Link>(linkM.LinkNames.FirstOrDefault(x => x.Value == (string)LinksList.SelectedItem).Key));
        }

        private void MapButtonFired(object sender, RoutedEventArgs e)
        {
            Device a = linkM.GetDevice(viewedLink.DeviceBaseId);
            Device b = linkM.GetDevice(viewedLink.DeviceEndId);
            MapWindow map;
            if (viewedLink.HopCount > 0)
                map = new MapWindow(a.Latitude, a.Longitude, b.Latitude, b.Longitude);
            else
                map = new MapWindow(a.Latitude, a.Longitude);
            map.Show();
        }

        private void ElevationButtonFired(object sender, RoutedEventArgs e)
        {
            ElevationWindow elevation;
            if (viewedLink.HopCount > 0)
            {
                Device a = linkM.GetDevice(viewedLink.DeviceBaseId);
                Device b = linkM.GetDevice(viewedLink.DeviceEndId);
                elevation = new ElevationWindow(a.Latitude, a.Longitude, b.Latitude, b.Longitude);
            }
            else
                elevation = new ElevationWindow();
            elevation.Show();
        }

        private void StartButtonFired(object sender, RoutedEventArgs e)
        {
            workerM.StartDevice(linkM.GetDevice(viewedDeviceId));
            ChartsChangeDevice(viewedDeviceId, linkM.GetDevice(viewedDeviceId));
            ContextChanged();
        }

        private void StopButtonFired(object sender, RoutedEventArgs e)
        {
            workerM.StopDevice(linkM.GetDevice(viewedDeviceId));
            ChartsChangeDevice(viewedDeviceId, linkM.GetDevice(viewedDeviceId));
        }

        private void ResetView()
        {
            ip.Content = String.Empty;
            unitname.Content = String.Empty;
            ping.Content = String.Empty;
            uptime.Content = String.Empty;
        }

        private void RegisterCharts()
        {
            signal.RegisterChart("Signal Level", "dBm", "[dBm]", dataM.measSig, viewedDeviceId, dataM.SignalTransactions, dataM);
            signalQ.RegisterChart("Signal Quality", "dB", "[dB]", dataM.measSigQ, viewedDeviceId, dataM.SignalQTransactions, dataM);
            tempOdu.RegisterChart("Outdoor Unit Temperature", "°C", "[°C]", dataM.measTmpO, viewedDeviceId, dataM.TempOduTransactions, dataM);
            tempIdu.RegisterChart("Outdoor Unit Temperature", "°C", "[°C]", dataM.measTmpI, viewedDeviceId, dataM.TempIduTransactions, dataM);
            voltage.RegisterChart("Power Voltage", "V", "[V]", dataM.measVolt, viewedDeviceId, dataM.VoltageTransactions, dataM);
            tx.RegisterChart("Transmit Data Rate", "kb/s", "[kbit/s]", dataM.measTx, viewedDeviceId, dataM.TxTransactions, dataM);
            rx.RegisterChart("Receive Data Rate", "kb/s", "[kbit/s]", dataM.measRx, viewedDeviceId, dataM.RxTransactions, dataM);
            latency.RegisterChart("Round/trip Time", "ms", "[ms]", dataM.measLat, viewedDeviceId, dataM.PingTransactions, dataM);
        }

        private void ChartsChangeDevice(int id, Device device)
        {
            ChartIdChanger(signal, id, device.IsEnabledSignal && !device.IsPaused);
            ChartIdChanger(signalQ, id, device.IsEnabledSignalQ && !device.IsPaused);
            ChartIdChanger(tempOdu, id, device.IsEnabledTempOdu && !device.IsPaused);
            ChartIdChanger(tempIdu, id, device.IsEnabledTempIdu && !device.IsPaused);
            ChartIdChanger(voltage, id, device.IsEnabledVoltage && !device.IsPaused);
            ChartIdChanger(tx, id, device.IsEnabledTx && !device.IsPaused);
            ChartIdChanger(rx, id, device.IsEnabledRx && !device.IsPaused);
            ChartIdChanger(latency, id, true && !device.IsPaused);
        }

        private void ChartIdChanger(ChartRealtimePane chart, int id, bool state)
        {
            if (state)
                chart.DeviceId = id;
            else
                chart.DeviceId = 0;
        }

        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BitmapImage result = new BitmapImage();
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }
    }
}
