using MicrowaveMonitor.Database;
using MicrowaveMonitor.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MicrowaveMonitor.Gui
{
    public partial class MonitoringWindow : Window
    {
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
            alarmListPane.SetItemsSource(alarmManager, linkManager, this);
            ChangeLink(linkManager.GetLink(linkManager.LinkNames.First().Key));

            foreach (string name in linkManager.LinkNames.Values)
            {
                LinksList.Items.Add(name);
            }

            LinksList.SelectedItem = linkManager.LinkNames.First().Value;
            LinksList.SelectionChanged += LinkChoosed;

            deviceAlarms.SetItemsSource(alarmM.deviceAlarms);

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

        public void ChangeLink(Link link)
        {
            ChangeLink(link, "A");
        }

        public void ChangeLink(Link link, string deviceLabel)
        {
            viewedLink = link;
            linkCaption.Content = link.Name;
            FillSettings();
            deviceTabs.SelectedIndex = 0;

            ChangeDevice(deviceLabel);
        }

        public void ChangeDevice(string deviceLabel)
        {
            int newDeviceId;
            RadioButton checkedRb;

            switch (deviceLabel)
            {
                case "A":
                    newDeviceId = viewedLink.DeviceBaseId;
                    checkedRb = siteA;
                    break;
                case "R1":
                    newDeviceId = viewedLink.DeviceR1Id;
                    checkedRb = siteR1;
                    break;
                case "R2":
                    newDeviceId = viewedLink.DeviceR2Id;
                    checkedRb = siteR2;
                    break;
                case "R3":
                    newDeviceId = viewedLink.DeviceR3Id;
                    checkedRb = siteR3;
                    break;
                case "R4":
                    newDeviceId = viewedLink.DeviceR4Id;
                    checkedRb = siteR4;
                    break;
                case "B":
                    newDeviceId = viewedLink.DeviceEndId;
                    checkedRb = siteB;
                    break;
                default:
                    throw new NotSupportedException();
            }

            if (newDeviceId == 0)
            {
                if (deviceLabel != "A" && viewedLink.DeviceBaseId > 0)
                {
                    newDeviceId = viewedLink.DeviceBaseId;
                    checkedRb = siteA;
                }
                else
                    return;
            }

            if (newDeviceId == viewedDeviceId)
                return;

            if (viewedDeviceId != 0)
            {
                ResetView();
                devicesDisplays[viewedDeviceId].PropertyChanged -= DataChangedDispatch;
            }

            viewedDeviceId = newDeviceId;

            if (checkedRb.IsChecked == false)
            {
                checkedRb.Checked -= SiteChoosed;
                checkedRb.IsChecked = true;
                checkedRb.Checked += SiteChoosed;
            }

            Device device = linkM.GetDevice(newDeviceId);
            ChartsChangeDevice(newDeviceId, device);

            ContextChanged();
            ShowStatics(device);
            if (monitorTabControl.SelectedIndex == 5)
                alarmM.FillDeviceAlarms(newDeviceId);

            devicesDisplays[newDeviceId].PropertyChanged += DataChangedDispatch;
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

            settingsA.FillBoxes(linkM.GetDevice(viewedLink.DeviceBaseId));
            siteB.IsEnabled = viewedLink.DeviceEndId > 0 ? true : false;
            checkB.IsChecked = viewedLink.DeviceEndId > 0 ? settingsB.FillBoxes(linkM.GetDevice(viewedLink.DeviceEndId)) : settingsB.ClearBoxes();
            siteR1.IsEnabled = viewedLink.DeviceR1Id > 0 ? true : false;
            checkR1.IsChecked = viewedLink.DeviceR1Id > 0 ? settingsR1.FillBoxes(linkM.GetDevice(viewedLink.DeviceR1Id)) : settingsR1.ClearBoxes();
            siteR2.IsEnabled = viewedLink.DeviceR2Id > 0 ? true : false;
            checkR2.IsChecked = viewedLink.DeviceR2Id > 0 ? settingsR2.FillBoxes(linkM.GetDevice(viewedLink.DeviceR2Id)) : settingsR2.ClearBoxes();
            siteR3.IsEnabled = viewedLink.DeviceR3Id > 0 ? true : false;
            checkR3.IsChecked = viewedLink.DeviceR3Id > 0 ? settingsR3.FillBoxes(linkM.GetDevice(viewedLink.DeviceR3Id)) : settingsR3.ClearBoxes();
            siteR4.IsEnabled = viewedLink.DeviceR4Id > 0 ? true : false;
            checkR4.IsChecked = viewedLink.DeviceR4Id > 0 ? settingsR4.FillBoxes(linkM.GetDevice(viewedLink.DeviceR4Id)) : settingsR4.ClearBoxes();
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
                        string tempAir = display.WeatherTemp != null ? display.WeatherTemp.ToString() : "-";
                        weatherTemp.Content = String.Format("{0:0} °C", tempAir);
                        break;
                    case "WeatherWind":
                        string wind = display.WeatherWind != null ? display.WeatherWind.ToString() : "-";
                        weatherWind.Content = wind + " m/s";
                        break;
                    default:
                        break;
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
                    linkState.Foreground = System.Windows.Media.Brushes.Blue;
                    buttonStart.IsEnabled = false;
                    buttonPause.IsEnabled = true;
                    break;
                case DeviceDisplay.LinkState.AlarmCritical:
                    linkState.Content = "critical";
                    linkState.Foreground = System.Windows.Media.Brushes.Purple;
                    buttonStart.IsEnabled = false;
                    buttonPause.IsEnabled = true;
                    break;
                case DeviceDisplay.LinkState.AlarmDown:
                    linkState.Content = "down";
                    linkState.Foreground = System.Windows.Media.Brushes.DarkRed;
                    buttonStart.IsEnabled = false;
                    buttonPause.IsEnabled = true;
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
            ChangeLink(linkM.GetLink(linkM.LinkNames.FirstOrDefault(x => x.Value == (string)e.AddedItems[0]).Key));
        }

        private void MapButtonFired(object sender, RoutedEventArgs e)
        {
            Device a = linkM.GetDevice(viewedLink.DeviceBaseId);
            MapWindow map;

            if (viewedLink.HopCount < 1)
                map = new MapWindow(a.Latitude, a.Longitude);
            else
            {
                Device b = linkM.GetDevice(viewedLink.DeviceEndId);
                map = new MapWindow(a.Latitude, a.Longitude, b.Latitude, b.Longitude);
            }

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

        private void NewButtonFired(object sender, RoutedEventArgs e)
        {
            NewLinkWindow newLinkWindow = new NewLinkWindow();
            if (newLinkWindow.ShowDialog() == true)
            {
                string name = newLinkWindow.NameAnswer;

                Device newDevice = new Device() { IsPaused = true };
                linkM.AddDevice(newDevice);
                workerM.InitDevice(newDevice);

                Link newLink = new Link() { Name = name, DeviceBaseId = newDevice.Id };
                linkM.AddLink(newLink);

                LinksList.Items.Add(newLink.Name);
                LinksList.SelectedItem = newLink.Name;
                monitorTabControl.SelectedIndex = 6;
                linksScroll.ScrollToBottom();
            }
        }

        private void DeleteButtonFired(object sender, RoutedEventArgs e)
        {
            DeleteWindow deleteWindow = new DeleteWindow();
            if (deleteWindow.ShowDialog() == true)
            {
                linkM.DeleteLink(viewedLink);

                switch (viewedLink.HopCount)
                {
                    case 0:
                        Device devBase = linkM.GetDevice(viewedLink.DeviceBaseId);
                        workerM.RemoveDevice(devBase);
                        linkM.DeleteDevice(devBase);
                        break;
                    case 1:
                        Device devEnd = linkM.GetDevice(viewedLink.DeviceEndId);
                        workerM.RemoveDevice(devEnd);
                        linkM.DeleteDevice(devEnd);
                        goto case 0;
                    case 2:
                        Device devR1 = linkM.GetDevice(viewedLink.DeviceR1Id);
                        workerM.RemoveDevice(devR1);
                        linkM.DeleteDevice(devR1);
                        goto case 1;
                    case 3:
                        Device devR2 = linkM.GetDevice(viewedLink.DeviceR2Id);
                        workerM.RemoveDevice(devR2);
                        linkM.DeleteDevice(devR2);
                        goto case 2;
                    case 4:
                        Device devR3 = linkM.GetDevice(viewedLink.DeviceR3Id);
                        workerM.RemoveDevice(devR3);
                        linkM.DeleteDevice(devR3);
                        goto case 3;
                    case 5:
                        Device devR4 = linkM.GetDevice(viewedLink.DeviceR4Id);
                        workerM.RemoveDevice(devR4);
                        linkM.DeleteDevice(devR4);
                        goto case 4;
                    default:
                        break;
                }

                string removing = viewedLink.Name;
                viewedDeviceId = 0;
                LinksList.SelectedItem = linkM.LinkNames.First().Value;
                LinksList.Items.Remove(removing);
                linksScroll.ScrollToTop();
            }
        }

        private void SettingsButtonFired(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(alarmM);
            settingsWindow.ShowDialog();
        }

        private void AboutButtonFired(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void SaveButtonFired(object sender, RoutedEventArgs e)
        {
            Device basedev = settingsA.SaveBoxes(linkM.GetDevice(viewedLink.DeviceBaseId));
            linkM.UpdateDevice(basedev);
            workerM.RestartDevice(basedev);

            byte hopCount = 0;

            if (boxLinkName.Text != string.Empty)
            {
                linkM.LinkNames[viewedLink.Id] = boxLinkName.Text;
                viewedLink.Name = boxLinkName.Text;
                LinksList.SelectionChanged -= LinkChoosed;
                LinksList.Items[LinksList.SelectedIndex] = boxLinkName.Text;
                LinksList.SelectedItem = boxLinkName.Text;
                LinksList.SelectionChanged += LinkChoosed;
            }
            viewedLink.Note = boxNote.Text;

            viewedLink.DeviceEndId = UpdateDeviceSettings(viewedLink.DeviceEndId, checkB, settingsB, 1, ref hopCount);
            viewedLink.DeviceR1Id = UpdateDeviceSettings(viewedLink.DeviceR1Id, checkR1, settingsR1, 2, ref hopCount);
            viewedLink.DeviceR2Id = UpdateDeviceSettings(viewedLink.DeviceR2Id, checkR2, settingsR2, 3, ref hopCount);
            viewedLink.DeviceR3Id = UpdateDeviceSettings(viewedLink.DeviceR3Id, checkR3, settingsR3, 4, ref hopCount);
            viewedLink.DeviceR4Id = UpdateDeviceSettings(viewedLink.DeviceR4Id, checkR4, settingsR4, 5, ref hopCount);
            viewedLink.HopCount = hopCount;

            linkM.UpdateLink(viewedLink);
            ChangeLink(viewedLink);
            Console.WriteLine("0Configuration saved. Link monitoring restarted.");
        }

        private void SearchButtonFired(object sender, RoutedEventArgs e)
        {
            LinksList.SelectionChanged -= LinkChoosed;
            LinksList.Items.Clear();

            foreach (string name in linkM.LinkNames.Values)
            {
                if (name.IndexOf(searchBox.Text, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    LinksList.Items.Add(name);
                }
            }

            LinksList.SelectionChanged += LinkChoosed;
        }

        private void ClearSearchButtonFired(object sender, RoutedEventArgs e)
        {
            searchBox.Text = string.Empty;
            LinksList.SelectionChanged -= LinkChoosed;
            LinksList.Items.Clear();

            foreach (string name in linkM.LinkNames.Values)
            {
                LinksList.Items.Add(name);
            }

            LinksList.SelectedItem = viewedLink.Name;
            LinksList.SelectionChanged += LinkChoosed;
        }

        private int UpdateDeviceSettings(int deviceId, CheckBox check, DeviceSettingsPane deviceSettings, int tabIndex, ref byte hops)
        {
            if ((deviceId > 0) && (bool)check.IsChecked)
            {
                Device dev = deviceSettings.SaveBoxes(linkM.GetDevice(deviceId));
                linkM.UpdateDevice(dev);
                workerM.RestartDevice(dev);
                hops++;
            }
            else if ((deviceId == 0) && (bool)check.IsChecked)
            {
                Device newDevice = new Device() { IsPaused = true };
                deviceSettings.SaveBoxes(newDevice);
                linkM.AddDevice(newDevice);
                workerM.InitDevice(newDevice);
                deviceId = newDevice.Id;
                hops++;
            }
            else if ((deviceId > 0) && !(bool)check.IsChecked)
            {
                Device delDevice = linkM.GetDevice(deviceId);
                if (viewedDeviceId == delDevice.Id)
                {
                    viewedDeviceId = 0;
                    siteA.IsChecked = true;
                }
                workerM.RemoveDevice(delDevice);
                linkM.DeleteDevice(delDevice);
                deviceId = 0;
                if (deviceTabs.SelectedIndex == tabIndex)
                    deviceTabs.SelectedIndex = 0;
            }

            return deviceId;
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
            signal.RegisterChart("Signal Strength", "dBm", "[dBm]", DataManager.measSig, viewedDeviceId, dataM.SignalTransactions, dataM);
            signalQ.RegisterChart("Signal Quality", "dB", "[dB]", DataManager.measSigQ, viewedDeviceId, dataM.SignalQTransactions, dataM);
            tempOdu.RegisterChart("Outdoor Unit Temperature", "°C", "[°C]", DataManager.measTmpO, viewedDeviceId, dataM.TempOduTransactions, dataM);
            tempIdu.RegisterChart("Indoor Unit Temperature", "°C", "[°C]", DataManager.measTmpI, viewedDeviceId, dataM.TempIduTransactions, dataM);
            voltage.RegisterChart("Power Voltage", "V", "[V]", DataManager.measVolt, viewedDeviceId, dataM.VoltageTransactions, dataM);
            tx.RegisterChart("Transmit Data Rate", "kb/s", "[kbit/s]", DataManager.measTx, viewedDeviceId, dataM.TxTransactions, dataM);
            rx.RegisterChart("Receive Data Rate", "kb/s", "[kbit/s]", DataManager.measRx, viewedDeviceId, dataM.RxTransactions, dataM);
            latency.RegisterChart("Round/trip Time", "ms", "[ms]", DataManager.measLat, viewedDeviceId, dataM.PingTransactions, dataM);
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

        private void DeviceAlarmsTabSelected(object sender, RoutedEventArgs e)
        {
            alarmM.FillDeviceAlarms(viewedDeviceId);
        }

        private void ListBoxMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                linksScroll.LineDown();
            }
            else
            {
                linksScroll.LineUp();
            }
        }

        public void SelectNameInLinksList()
        {
            LinksList.SelectionChanged -= LinkChoosed;
            LinksList.SelectedItem = viewedLink.Name;
            LinksList.SelectionChanged += LinkChoosed;
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
