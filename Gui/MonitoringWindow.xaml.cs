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
using Vibrant.InfluxDB.Client;
using Vibrant.InfluxDB.Client.Rows;

namespace MicrowaveMonitor.Gui
{
    public partial class MonitoringWindow : Window
    {
        public enum DeviceType { Base, End, R1, R2, R3, R4 };

        private readonly LinkManager linkM;
        private readonly WorkerManager workerM;
        private readonly AlarmManager alarmM;
        private readonly DataManager dataM;

        public Dictionary<int, DeviceDisplay> devicesDisplays;
        public Link viewedLink;
        public int viewedDeviceId = 0;

        public MonitoringWindow(LinkManager linkManager, WorkerManager workerManager, AlarmManager alarmManager, DataManager dataManager)
        {
            linkM = linkManager;
            workerM = workerManager;
            alarmM = alarmManager;
            dataM = dataManager;
            devicesDisplays = workerManager.DeviceToFront;

            InitializeComponent();
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

            historySignal.SelectionChanged += HistoryChanged;
            historySignalQ.SelectionChanged += HistoryChanged;
            historyTempOdu.SelectionChanged += HistoryChanged;
            historyTempIdu.SelectionChanged += HistoryChanged;
            historyVoltage.SelectionChanged += HistoryChanged;
            historyTx.SelectionChanged += HistoryChanged;
            historyRx.SelectionChanged += HistoryChanged;
            historyLatency.SelectionChanged += HistoryChanged;

            signalLevel.axisY.Title = "[dBm]";
            signalQuality.axisY.Title = "[dB]";
            tx.axisY.Title = "[bit/s]";
            rx.axisY.Title = "[bit/s]";
            tx.axisY.MinValue = 0;
            rx.axisY.MinValue = 0;
            tempIdu.axisY.Title = "[°C]";
            tempOdu.axisY.Title = "[°C]";
            voltage.axisY.Title = "[V]";
            voltage.axisY.MinValue = 0;
            pingwin.axisY.Title = "[ms]";
            pingwin.axisY.MinValue = 0;
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

            HistoryChanged(historySignal, null);
            HistoryChanged(historySignalQ, null);
            HistoryChanged(historyTempOdu, null);
            HistoryChanged(historyTempIdu, null);
            HistoryChanged(historyVoltage, null);
            HistoryChanged(historyTx, null);
            HistoryChanged(historyRx, null);
            HistoryChanged(historyLatency, null);
            ShowStatics();

            devicesDisplays[viewedDeviceId].PropertyChanged += DataChangedDispatch;
        }

        private void ShowStatics()
        {
            ip.Content = linkM.GetDevice(viewedDeviceId).Address.ToString();
            DataChanged(null, new PropertyChangedEventArgs("SysName"));
            DataChanged(null, new PropertyChangedEventArgs("Uptime"));
            DataChanged(null, new PropertyChangedEventArgs("DiffPing"));
            DataChanged(null, new PropertyChangedEventArgs("DiffSig"));
            DataChanged(null, new PropertyChangedEventArgs("DiffSigQ"));
            DataChanged(null, new PropertyChangedEventArgs("WeatherIcon"));
            DataChanged(null, new PropertyChangedEventArgs("WeatherDesc"));
            DataChanged(null, new PropertyChangedEventArgs("WeatherTemp"));
            DataChanged(null, new PropertyChangedEventArgs("WeatherWind"));
            if (devicesDisplays[viewedDeviceId].DataPing != null)
                DataChanged(null, new PropertyChangedEventArgs("DataPing"));
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

        private void DataChanged(object sender, PropertyChangedEventArgs e)
        {
            DeviceDisplay display = (DeviceDisplay)sender;
            
            if (display == devicesDisplays[viewedDeviceId])
                switch (e.PropertyName)
                {
                    case "DiffPing":
                        diffPing.Content = String.Format("{0:0.0000} ms", display.DiffPing);
                        avgPing.Content = String.Format("{0:0.00} ms", display.AvgPing);
                        break;
                    case "DiffSig":
                        diffSig.Content = String.Format("{0:0.0000} dBm", display.DiffSig);
                        avgSig.Content = String.Format("{0:0.00} dBm", display.AvgSig);
                        break;
                    case "DiffSigQ":
                        diffSigQ.Content = String.Format("{0:0.0000} dB", display.DiffSigQ);
                        avgSigQ.Content = String.Format("{0:0.00} dB", display.AvgSigQ);
                        break;
                    case "SysName":
                        unitname.Content = display.SysName;
                        break;
                    case "Uptime":
                        TimeSpan t = TimeSpan.FromSeconds(display.Uptime / 100);
                        uptime.Content = String.Format("{0:D2}d {1:D2}h {2:D2}m {3:D2}s", t.Days, t.Hours, t.Minutes, t.Seconds);
                        break;
                    case "DataPing":
                        ping.Content = String.Format("{0} ms", display.DataPing.Data);
                        GraphUpdate(pingwin, historyLatency, display.DataPing, null);
                        break;
                    case "DataSig":
                        GraphUpdate(signalLevel, historySignal, display.DataSig, null);
                        break;
                    case "DataSigQ":
                        GraphUpdate(signalQuality, historySignalQ, display.DataSigQ, null);
                        break;
                    case "DataTx":
                        var convertedTx = new Record<double>(display.DataTx.TimeMark, display.DataTx.Data);
                        GraphUpdate(tx, historyTx, convertedTx, null);
                        break;
                    case "DataRx":
                        var convertedRx = new Record<double>(display.DataRx.TimeMark, display.DataRx.Data);
                        GraphUpdate(rx, historyRx, convertedRx, null);
                        break;
                    case "DataTempOdu":
                        GraphUpdate(tempOdu, historyTempOdu, display.DataTempOdu, null);
                        break;
                    case "DataTempIdu":
                        GraphUpdate(tempIdu, historyTempIdu, display.DataTempIdu, null);
                        break;
                    case "DataVoltage":
                        GraphUpdate(voltage, historyVoltage, display.DataVoltage, null);
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

        private void GraphUpdate(GraphRealtime graph, ComboBox history, Record<double> record, List<Record<double>> manyRecords)
        {
            int resolution;     // chart's samples count
            int span;           // chart's timespan (s)

            switch (history.SelectedIndex)
            {
                case 0:         // 1 m
                    resolution = 60;
                    span = 60;
                    break;
                case 1:         // 10 m
                    resolution = 600;
                    span = 600;
                    break;
                default:
                    return;
            }

            if (record != null && span == graph.Span && viewedDeviceId == graph.DevId)
                graph.Read(record, resolution, span);
            else if (manyRecords != null && manyRecords.Count > 0)
                graph.ReadMany(manyRecords, resolution, span, viewedDeviceId);
            else
                graph.SetAxisLimits(DateTime.Now, span);
        }

        private async void HistoryChanged(object sender, SelectionChangedEventArgs e)
        {
            long step;
            long unit;
            string query;

            ComboBox history = (ComboBox)sender;
            switch (history.SelectedIndex)
            {
                case 0:         // 1 m
                    step = TimeSpan.FromSeconds(5).Ticks;
                    unit = TimeSpan.TicksPerSecond;
                    query = $@"WHERE time > now() - 1m AND time < now() AND ""device""='{viewedDeviceId}' GROUP BY time(1s) FILL(none)";
                    break;
                case 1:         // 10 m
                    step = TimeSpan.FromSeconds(50).Ticks;
                    unit = TimeSpan.TicksPerSecond;
                    query = $@"WHERE time > now() - 10m AND time < now() AND ""device""='{viewedDeviceId}' GROUP BY time(1s) FILL(none)";
                    break;
                default:
                    return;
            }

            GraphRealtime graph;
            DynamicInfluxRow[] pending;

            string pre = $@"SELECT mean(""value"") FROM ""{DataManager.databaseName}"".""{DataManager.retention}"".";

            switch (history.Tag)
            {
                case "sig":
                    graph = signalLevel;
                    lock (dataM.SignalTransactions)
                        pending = dataM.SignalTransactions.ToArray();
                    query = pre + $@"""{DataManager.measSig}"" " + query;
                    break;
                case "sigQ":
                    graph = signalQuality;
                    lock (dataM.SignalQTransactions)
                        pending = dataM.SignalQTransactions.ToArray();
                    query = pre + $@"""{DataManager.measSigQ}"" " + query;
                    break;
                case "tempodu":
                    graph = tempOdu;
                    lock (dataM.TempOduTransactions)
                        pending = dataM.TempOduTransactions.ToArray();
                    query = pre + $@"""{DataManager.measTmpO}"" " + query;
                    break;
                case "tempidu":
                    graph = tempIdu;
                    lock (dataM.TempIduTransactions)
                        pending = dataM.TempIduTransactions.ToArray();
                    query = pre + $@"""{DataManager.measTmpI}"" " + query;
                    break;
                case "volt":
                    graph = voltage;
                    lock (dataM.VoltageTransactions)
                        pending = dataM.VoltageTransactions.ToArray();
                    query = pre + $@"""{DataManager.measVolt}"" " + query;
                    break;
                case "tx":
                    graph = tx;
                    lock (dataM.TxTransactions)
                        pending = dataM.TxTransactions.ToArray();
                    query = pre + $@"""{DataManager.measTx}"" " + query;
                    break;
                case "rx":
                    graph = rx;
                    lock (dataM.RxTransactions)
                        pending = dataM.RxTransactions.ToArray();
                    query = pre + $@"""{DataManager.measRx}"" " + query;
                    break;
                case "ping":
                    graph = pingwin;
                    lock (dataM.PingTransactions)
                        pending = dataM.PingTransactions.ToArray();
                    query = pre + $@"""{DataManager.measLat}"" " + query;
                    break;
                default:
                    return;
            }

            graph.ChartValues.Clear();
            graph.SetAxisGrid(step, unit);

            List<Record<double>> records = new List<Record<double>>();
            InfluxSeries<DynamicInfluxRow> series = await dataM.QuerySeries(query);
            if (series != null)            
                foreach (dynamic row in series.Rows)
                {
                    records.Add(new Record<double>(row.time.ToLocalTime(), row.mean));
                }

            try
            {
                foreach (dynamic row in pending)
                {
                    if (row.device == viewedDeviceId.ToString())
                    {
                        DateTime timestamp = row.Timestamp;
                        records.Add(new Record<double>(timestamp.ToLocalTime(), row.value));
                    }
                }
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException ex)
            {
                Console.WriteLine(ex.Message);
            }

            GraphUpdate(graph, history, null, records);
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

        private void ButtonFired(object sender, RoutedEventArgs e)
        {
            Button source = (Button)sender;

            switch (source.Name)
            {
                case "buttonMap":
                    MapWindow map;
                    if (viewedLink.HopCount > 0)
                        map = new MapWindow(linkM.GetDevice(viewedLink.DeviceBaseId).Latitude, linkM.GetDevice(viewedLink.DeviceBaseId).Longitude, linkM.GetDevice(viewedLink.DeviceEndId).Latitude, linkM.GetDevice(viewedLink.DeviceEndId).Longitude);
                    else
                        map = new MapWindow(linkM.GetDevice(viewedLink.DeviceBaseId).Latitude, linkM.GetDevice(viewedLink.DeviceBaseId).Longitude);
                    map.Show();
                    break;
                case "buttonTerrain":
                    ElevationWindow elevation;
                    if (viewedLink.HopCount > 0)
                        elevation = new ElevationWindow(linkM.GetDevice(viewedLink.DeviceBaseId).Latitude, linkM.GetDevice(viewedLink.DeviceBaseId).Longitude, linkM.GetDevice(viewedLink.DeviceEndId).Latitude, linkM.GetDevice(viewedLink.DeviceEndId).Longitude);
                    else
                        elevation = new ElevationWindow();
                    elevation.Show();
                    break;
                default:
                    break;
            }
        }

        private void ResetView()
        {
            ip.Content = String.Empty;
            unitname.Content = String.Empty;
            ping.Content = String.Empty;
            uptime.Content = String.Empty;
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
