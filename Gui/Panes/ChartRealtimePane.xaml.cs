using MicrowaveMonitor.Database;
using MicrowaveMonitor.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.CSharp.RuntimeBinder;
using Vibrant.InfluxDB.Client.Rows;

namespace MicrowaveMonitor.Gui
{
    public partial class ChartRealtimePane : UserControl
    {
        private struct QueryParams
        {
            public string valueName;
            public string retention;
            public string bottomTime;
        }

        QueryParams queryOneMin = new QueryParams()
        {
            valueName = DataManager.defaultValueName,
            retention = DataManager.retentionWeek,
            bottomTime = "1m"
        };
        QueryParams queryTenMin = new QueryParams()
        {
            valueName = DataManager.defaultValueName,
            retention = DataManager.retentionWeek,
            bottomTime = "10m"
        };
        QueryParams queryOneHour = new QueryParams()
        {
            valueName = DataManager.defaultValueName,
            retention = DataManager.retentionWeek,
            bottomTime = "1h"
        };
        QueryParams querySixHours = new QueryParams()
        {
            valueName = DataManager.defaultValueName,
            retention = DataManager.retentionWeek,
            bottomTime = "6h"
        };
        QueryParams queryOneDay = new QueryParams()
        {
            valueName = DataManager.defaultValueName,
            retention = DataManager.retentionWeek,
            bottomTime = "1d"
        };
        QueryParams queryOneWeek = new QueryParams()
        {
            valueName = DataManager.meanValueName,
            retention = DataManager.retentionMonth,
            bottomTime = "1w"
        };
        QueryParams queryOneMonth = new QueryParams()
        {
            valueName = DataManager.meanValueName,
            retention = DataManager.retentionYear,
            bottomTime = "30d"
        };
        QueryParams queryOneYear = new QueryParams()
        {
            valueName = DataManager.meanValueName,
            retention = DataManager.retentionYear,
            bottomTime = "365d"
        };

        private int _deviceId;
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
        public string ChartName { get; set; }
        public string Unit { get; set; }
        public string AxisUnit { get; set; }
        public string Measurement { get; set; }
        public Queue<DynamicInfluxRow> Transactions { get; set; }
        public DataManager DataM { get; set; }
        public double Avg { get; set; }
        public double Diff { get; set; }

        private bool isFilled = false;
        private int writesCount = 0;
        private double valueSum = 0;
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
            QueryParams avgQueryParams;

            const int avgUpdateTrigger = 10;
            TimeSpan chartUpdateInterval;

            switch (history.SelectedIndex)
            {
                case 0:         // 1 m
                    resolution = 60;
                    span = 60;
                    avgQueryParams = queryOneMin;
                    chartUpdateInterval = TimeSpan.FromSeconds(1);
                    break;
                case 1:         // 10 m
                    resolution = 600;
                    span = 600;
                    avgQueryParams = queryTenMin;
                    chartUpdateInterval = TimeSpan.FromSeconds(1);
                    break;
                case 2:         // 1 h
                    resolution = 600;
                    span = 3600;
                    avgQueryParams = queryOneHour;
                    chartUpdateInterval = TimeSpan.FromSeconds(6);
                    break;
                case 3:         // 6 h
                    resolution = 600;
                    span = 21600;
                    avgQueryParams = querySixHours;
                    chartUpdateInterval = TimeSpan.FromSeconds(36);
                    break;
                case 4:         // 1 d
                    resolution = 600;
                    span = 86400;
                    avgQueryParams = queryOneDay;
                    chartUpdateInterval = TimeSpan.FromSeconds(144);
                    break;
                case 5:         // 1 w
                    resolution = 600;
                    span = 604800;
                    avgQueryParams = queryOneWeek;
                    chartUpdateInterval = TimeSpan.FromSeconds(1008);
                    break;
                case 6:         // 30 d
                    resolution = 600;
                    span = 2592000;
                    avgQueryParams = queryOneMonth;
                    chartUpdateInterval = TimeSpan.FromSeconds(4320);
                    break;
                case 7:         // 365 d
                    resolution = 600;
                    span = 31536000;
                    avgQueryParams = queryOneYear;
                    chartUpdateInterval = TimeSpan.FromSeconds(4320);
                    break;
                default:
                    return;
            }

            if (record != null && DeviceId == chart.DevId)
            {
                if (chartUpdateInterval.TotalSeconds > 1)
                {
                    valueSum += record.Data;
                    writesCount++;

                    if (DateTime.Now - lastUpdate > chartUpdateInterval)
                    {
                        Record<double> newRecord = new Record<double>(record.TimeMark, valueSum / writesCount);

                        chart.Read(newRecord, resolution, span);
                        await UpdateAvg(avgQueryParams);
                        UpdateDiff(record.Data);

                        lastUpdate = record.TimeMark;
                        valueSum = 0;
                        writesCount = 0;
                    }
                }
                else
                {
                    chart.Read(record, resolution, span);
                    writesCount++;

                    if (++writesCount % avgUpdateTrigger == 0)
                        await UpdateAvg(avgQueryParams);

                    UpdateDiff(record.Data);
                }
            }
            else if (manyRecords != null)
            {
                chart.ReadMany(manyRecords, resolution, span, DeviceId);
                isFilled = true;
                await UpdateAvg(avgQueryParams);
            }
            else
                chart.SetAxisLimits(DateTime.Now, span);
        }

        private async Task UpdateAvg(QueryParams parameters)
        {
            string query = $@"SELECT mean(""{parameters.valueName}"") FROM ""{DataManager.databaseName}"".""{parameters.retention}"".""{Measurement}"" WHERE time > now() - {parameters.bottomTime} AND time < now() AND ""device""='{DeviceId}'";

            dynamic row = await DataM.QueryValue(query);
            if (row != null)
            {
                avg.Content = String.Format("{0:0.00} {1}", row.mean, Unit);
                Avg = row.mean;
            }
        }

        private void UpdateDiff(double value)
        {
            Diff = value - Avg;
            diff.Content = String.Format("{0} {1}", Diff.ToString("+0.00;−0.00"), Unit);
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
            QueryParams histQueryParams;

            switch (history.SelectedIndex)
            {
                case 0:         // 1 m
                    step = 5;
                    unit = 1;
                    histQueryParams = queryOneMin;
                    break;
                case 1:         // 10 m
                    step = 50;
                    unit = 1;
                    histQueryParams = queryTenMin;
                    break;
                case 2:         // 1 h
                    step = 300;
                    unit = 6;
                    histQueryParams = queryOneHour;
                    break;
                case 3:         // 6 h
                    step = 1800;
                    unit = 36;
                    histQueryParams = querySixHours;
                    break;
                case 4:         // 1 d
                    step = 7200;
                    unit = 144;
                    histQueryParams = queryOneDay;
                    break;
                case 5:         // 1 w
                    step = 50400;
                    unit = 1008;
                    histQueryParams = queryOneWeek;
                    break;
                case 6:         // 30 d
                    step = 216000;
                    unit = 4320;
                    histQueryParams = queryOneMonth;
                    break;
                case 7:         // 365 d
                    step = 2628000;
                    unit = 52560;
                    histQueryParams = queryOneYear;
                    break;
                default:
                    return;
            }

            chart.SetAxisGrid(step, unit);

            DynamicInfluxRow[] pending;
            lock (Transactions)
                pending = Transactions.ToArray();

            List<Record<double>> records = new List<Record<double>>();

            string query = $@"SELECT mean(""{histQueryParams.valueName}"") FROM ""{DataManager.databaseName}"".""{histQueryParams.retention}"".""{Measurement}"" WHERE time > now() - {histQueryParams.bottomTime} AND time < now() AND ""device""='{DeviceId}' GROUP BY time({unit}s) FILL(none)";
            List<DynamicInfluxRow> rows = await DataM.QueryRows(query);

            if (rows != null)
                foreach (dynamic row in rows)
                {
                    records.Add(new Record<double>(row.time.ToLocalTime(), row.mean));
                }

            try
            {
                if (unit > 1)
                {
                    double sum = 0;
                    int count = 0;
                    DateTime time = DateTime.Now;

                    foreach (dynamic row in pending)
                    {
                        if (row.device == DeviceId.ToString())
                        {
                            sum += row.value;

                            if (count++ == 0)
                                time = row.Timestamp.ToLocalTime();

                            if ((row.Timestamp.ToLocalTime() - time).TotalSeconds > unit)
                            {
                                records.Add(new Record<double>(row.Timestamp.ToLocalTime(), sum / count));
                                sum = 0;
                                count = 0;
                                time = row.Timestamp.ToLocalTime();
                            }
                        }
                    }
                }
                else
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
            }
            catch (RuntimeBinderException ex)
            {
                Console.WriteLine(ex.Message);
            }

            writesCount = 0;
            valueSum = 0;
            await GraphUpdate(null, records);
        }
    }
}
