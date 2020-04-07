using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Configurations;
using MicrowaveMonitor.Database;
using MicrowaveMonitor.Managers;
using Vibrant.InfluxDB.Client;
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
                    disNotify.Visibility = Visibility.Visible;
                    IsEnabled = false;
                }
            }
        }
        public Queue<DynamicInfluxRow> Transactions { get; set; }
        public DataManager DataM { get; set; }
        public double Avg { get; set; }
        public double Diff { get; set; }

        private int writesCount = 0;

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
            int resolution;     // chart's samples count
            int span;           // chart's timespan (s)
            string query;

            const int trigger = 10;

            switch (history.SelectedIndex)
            {
                case 0:         // 1 m
                    resolution = 60;
                    span = 60;
                    query = $@"WHERE time > now() - 1m AND time < now() AND ""device""='{DeviceId}'";
                    break;
                case 1:         // 10 m
                    resolution = 600;
                    span = 600;
                    query = $@"WHERE time > now() - 10m AND time < now() AND ""device""='{DeviceId}'";
                    break;
                default:
                    return;
            }

            if (record != null && span == chart.Span && DeviceId == chart.DevId)
            {
                chart.Read(record, resolution, span);
                writesCount++;
                if (writesCount > trigger)
                    await UpdateAvg(query);

                Diff = record.Data - Avg;
                diff.Content = String.Format("{0} {1}", Diff.ToString("+0.00;−0.00"), Unit);
            }
            else if (manyRecords != null && manyRecords.Count > 0)
            {
                chart.ReadMany(manyRecords, resolution, span, DeviceId);
                await UpdateAvg(query);
            }
            else
                chart.SetAxisLimits(DateTime.Now, span);
        }

        private async Task UpdateAvg(string query)
        {
            string pre = $@"SELECT mean(""value"") FROM ""{DataM.databaseName}"".""{DataM.retention}"".";
            query = pre + $@"""{Measurement}"" " + query;

            dynamic row = await DataM.QueryValue(query);
            if (row != null)
            {
                avg.Content = String.Format("{0:0.00} {1}", row.mean, Unit);
                Avg = row.mean;
                writesCount = 0;
            }
        }

        public async void HistoryChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataM == null || Measurement == null)
                return;

            chart.ChartValues.Clear();
            avg.Content = string.Empty;
            diff.Content = string.Empty;

            long step;
            long unit;
            string query;

            switch (history.SelectedIndex)
            {
                case 0:         // 1 m
                    step = TimeSpan.FromSeconds(5).Ticks;
                    unit = TimeSpan.TicksPerSecond;
                    query = $@"WHERE time > now() - 1m AND time < now() AND ""device""='{DeviceId}' GROUP BY time(1s) FILL(none)";
                    break;
                case 1:         // 10 m
                    step = TimeSpan.FromSeconds(50).Ticks;
                    unit = TimeSpan.TicksPerSecond;
                    query = $@"WHERE time > now() - 10m AND time < now() AND ""device""='{DeviceId}' GROUP BY time(1s) FILL(none)";
                    break;
                default:
                    return;
            }

            query = $@"SELECT mean(""value"") FROM ""{DataM.databaseName}"".""{DataM.retention}""." + $@"""{Measurement}"" " + query;

            chart.SetAxisGrid(step, unit);

            if (DeviceId == 0)
                return;

            DynamicInfluxRow[] pending;
            lock (Transactions)
                pending = Transactions.ToArray();

            List<Record<double>> records = new List<Record<double>>();

            InfluxSeries<DynamicInfluxRow> series = await DataM.QuerySeries(query);
            if (series != null)
                foreach (dynamic row in series.Rows)
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

            await GraphUpdate(null, records);
        }
    }
}
