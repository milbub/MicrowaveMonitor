using MicrowaveMonitor.Database;
using MicrowaveMonitor.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Vibrant.InfluxDB.Client.Rows;

namespace MicrowaveMonitor.Gui
{
    public partial class ChartRealtimePane : UserControl
    {
        private int _deviceId;

        public string ChartName { get; set; }
        public string Unit { get; set; }
        public string AxisUnit { get; set; }
        public string Measurement { get; set; }
        public int DeviceId
        {
            get => _deviceId;
            set
            {
                if (value > 0)
                {
                    _deviceId = value;
                    disNotify.Visibility = Visibility.Hidden;
                    IsEnabled = true;
                }
                else
                {
                    _deviceId = 0;
                    isFilled = false;
                    chart.ChartValues.Clear();
                    avg.Content = string.Empty;
                    diff.Content = string.Empty;
                    disNotify.Visibility = Visibility.Visible;
                    IsEnabled = false;
                }
            }
        }
        public Queue<DynamicInfluxRow> Transactions { get; set; }
        public DataManager DataM { get; set; }
        public double Avg { get; set; }
        public double Diff { get; set; }

        private bool isFilled = false;
        private int writesCount = 0;
        private DateTime lastUpdate;

        public ChartRealtimePane()
        {
            InitializeComponent();
        }

        public void RegisterChart(string chartName, string unit, string axisUnit, string measurement, int deviceId, Queue<DynamicInfluxRow> transactions, DataManager dataM)
        {
            ChartName = chartName;
            Unit = unit;
            AxisUnit = axisUnit;
            Measurement = measurement;
            DeviceId = deviceId;
            Transactions = transactions;
            DataM = dataM;

            chart.axisY.Title = axisUnit;
            mark.Content = chartName;
        }

        public async Task GraphUpdate(Record<double> record, List<Record<double>> manyRecords)
        {
            if (manyRecords is null && !isFilled)
                return;

            int resolution;     // chart's samples count
            int span;           // chart's timespan (s)
            string query;

            const int avgUpdateTrigger = 10;
            TimeSpan chartUpdateInterval;

            switch (history.SelectedIndex)
            {
                case 0:         // 1 m
                    resolution = 60;
                    span = 60;
                    query = $@"SELECT mean(""value"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionWeek}"".""{Measurement}"" WHERE time > now() - 1m AND time < now() AND ""device""='{DeviceId}'";
                    chartUpdateInterval = TimeSpan.FromSeconds(1);
                    break;
                case 1:         // 10 m
                    resolution = 600;
                    span = 600;
                    query = $@"SELECT mean(""value"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionWeek}"".""{Measurement}"" WHERE time > now() - 10m AND time < now() AND ""device""='{DeviceId}'";
                    chartUpdateInterval = TimeSpan.FromSeconds(1);
                    break;
                case 2:         // 1 h
                    resolution = 600;
                    span = 3600;
                    query = $@"SELECT mean(""value"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionWeek}"".""{Measurement}"" WHERE time > now() - 1h AND time < now() AND ""device""='{DeviceId}'";
                    chartUpdateInterval = TimeSpan.FromSeconds(6);
                    break;
                case 3:         // 6 h
                    resolution = 600;
                    span = 21600;
                    query = $@"SELECT mean(""value"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionWeek}"".""{Measurement}"" WHERE time > now() - 6h AND time < now() AND ""device""='{DeviceId}'";
                    chartUpdateInterval = TimeSpan.FromSeconds(36);
                    break;
                case 4:         // 1 d
                    resolution = 600;
                    span = 86400;
                    query = $@"SELECT mean(""value"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionWeek}"".""{Measurement}"" WHERE time > now() - 1d AND time < now() AND ""device""='{DeviceId}'";
                    chartUpdateInterval = TimeSpan.FromSeconds(144);
                    break;
                case 5:         // 1 w
                    resolution = 600;
                    span = 604800;
                    query = $@"SELECT mean(""value"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionWeek}"".""{Measurement}"" WHERE time > now() - 1w AND time < now() AND ""device""='{DeviceId}'";
                    chartUpdateInterval = TimeSpan.FromSeconds(1008);
                    break;
                case 6:         // 30 d
                    resolution = 600;
                    span = 2592000;
                    query = $@"SELECT mean(""mean_value"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionMonth}"".""{Measurement}"" WHERE time > now() - 30d AND time < now() AND ""device""='{DeviceId}'";
                    chartUpdateInterval = TimeSpan.FromSeconds(4320);
                    break;
                case 7:         // 365 d
                    resolution = 600;
                    span = 31536000;
                    query = $@"SELECT mean(""mean_value"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionYear}"".""{Measurement}"" WHERE time > now() - 365d AND time < now() AND ""device""='{DeviceId}'";
                    chartUpdateInterval = TimeSpan.FromSeconds(4320);
                    break;
                default:
                    return;
            }

            if (record != null && DeviceId == chart.DevId)
            {
                DateTime now = DateTime.Now;

                if (now - lastUpdate > chartUpdateInterval)
                {
                    chart.Read(record, resolution, span);
                    lastUpdate = now;
                }

                if (++writesCount % avgUpdateTrigger == 0)
                    await UpdateAvg(query);

                Diff = record.Data - Avg;
                diff.Content = String.Format("{0} {1}", Diff.ToString("+0.00;−0.00"), Unit);
            }
            else if (manyRecords != null)
            {
                chart.ReadMany(manyRecords, resolution, span, DeviceId);
                isFilled = true;
                await UpdateAvg(query);
            }
            else
                chart.SetAxisLimits(DateTime.Now, span);
        }

        private async Task UpdateAvg(string query)
        {
            dynamic row = await DataM.QueryValue(query);
            if (row != null)
            {
                avg.Content = String.Format("{0:0.00} {1}", row.mean, Unit);
                Avg = row.mean;
            }
        }

        public async void HistoryChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataM == null || Measurement == null)
                return;

            isFilled = false;
            chart.ChartValues.Clear();
            avg.Content = string.Empty;
            diff.Content = string.Empty;

            int step;
            int unit;
            string query;

            switch (history.SelectedIndex)
            {
                case 0:         // 1 m
                    step = 5;
                    unit = 1;
                    query = $@"SELECT mean(""value"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionWeek}"".""{Measurement}"" WHERE time > now() - 1m AND time < now() AND ""device""='{DeviceId}' GROUP BY time(1s) FILL(none)";
                    break;
                case 1:         // 10 m
                    step = 50;
                    unit = 1;
                    query = $@"SELECT mean(""value"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionWeek}"".""{Measurement}"" WHERE time > now() - 10m AND time < now() AND ""device""='{DeviceId}' GROUP BY time(1s) FILL(none)";
                    break;
                case 2:         // 1 h
                    step = 300;
                    unit = 6;
                    query = $@"SELECT mean(""value"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionWeek}"".""{Measurement}"" WHERE time > now() - 1h AND time < now() AND ""device""='{DeviceId}' GROUP BY time(6s) FILL(none)";
                    break;
                case 3:         // 6 h
                    step = 1800;
                    unit = 36;
                    query = $@"SELECT mean(""value"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionWeek}"".""{Measurement}"" WHERE time > now() - 6h AND time < now() AND ""device""='{DeviceId}' GROUP BY time(36s) FILL(none)";
                    break;
                case 4:         // 1 d
                    step = 7200;
                    unit = 144;
                    query = $@"SELECT mean(""value"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionWeek}"".""{Measurement}"" WHERE time > now() - 1d AND time < now() AND ""device""='{DeviceId}' GROUP BY time(144s) FILL(none)";
                    break;
                case 5:         // 1 w
                    step = 50400;
                    unit = 1008;
                    query = $@"SELECT mean(""value"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionWeek}"".""{Measurement}"" WHERE time > now() - 1w AND time < now() AND ""device""='{DeviceId}' GROUP BY time(1008s) FILL(none)";
                    break;
                case 6:         // 30 d
                    step = 216000;
                    unit = 4320;
                    query = $@"SELECT mean(""mean_value"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionMonth}"".""{Measurement}"" WHERE time > now() - 30d AND time < now() AND ""device""='{DeviceId}' GROUP BY time(4320s) FILL(none)";
                    break;
                case 7:         // 365 d
                    step = 2628000;
                    unit = 52560;
                    query = $@"SELECT mean(""mean_value"") FROM ""{DataManager.databaseName}"".""{DataManager.retentionYear}"".""{Measurement}"" WHERE time > now() - 365d AND time < now() AND ""device""='{DeviceId}' GROUP BY time(52560s) FILL(none)";
                    break;
                default:
                    return;
            }

            chart.SetAxisGrid(step, unit);

            DynamicInfluxRow[] pending;
            lock (Transactions)
                pending = Transactions.ToArray();

            List<Record<double>> records = new List<Record<double>>();

            List<DynamicInfluxRow> rows = await DataM.QueryRows(query);

            if (rows != null)
                foreach (dynamic row in rows)
                {
                    records.Add(new Record<double>(row.time.ToLocalTime(), row.mean));
                }

            try
            {
                foreach (dynamic row in pending)
                {
                    if (row.device == DeviceId.ToString())
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

            writesCount = 0;
            await GraphUpdate(null, records);
        }
    }
}
