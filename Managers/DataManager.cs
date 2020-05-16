using MicrowaveMonitor.Workers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client;
using Vibrant.InfluxDB.Client.Rows;

namespace MicrowaveMonitor.Managers
{
    public class DataManager
    {
        public static int DbWriteInterval { get; } = 10000;      // 10 sec
        private static float weatherCycles = WeatherCollector.MinRefresh / DbWriteInterval;

        public const int contQueryCycle = 30000;                 // 30 sec

        public const string measLat = "latency";
        public const string measSig = "signal";
        public const string measSigQ = "signalQ";
        public const string measTx = "tx";
        public const string measRx = "rx";
        public const string measTmpO = "tempOdu";
        public const string measTmpI = "tempIdu";
        public const string measVolt = "voltage";
        public const string measTmpA = "tempAir";
        public const string measWeath = "weather";

        public const string defaultValueName = "value";
        public const string meanValueName = "mean_value";
        public const string defaultWindName = "wind";
        public const string meanWindName = "mean_wind";
        public const string defaultCondiName = "condition";
        public const string medianCondiName = "median_condition";

        public Queue<DynamicInfluxRow> PingTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> SignalTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> SignalQTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> TxTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> RxTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> TempOduTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> TempIduTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> VoltageTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> WeatherTempTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> WeatherOtherTransactions = new Queue<DynamicInfluxRow>();

        public static string serverAddress = ConfigurationManager.ConnectionStrings["InfluxServer"].ConnectionString;
        public static string databaseName = ConfigurationManager.ConnectionStrings["InfluxData"].ConnectionString;
        public static string writeRetention = ConfigurationManager.ConnectionStrings["InfluxWriteRetention"].ConnectionString;
        private static readonly string user = ConfigurationManager.ConnectionStrings["InfluxUser"].ConnectionString;
        private static readonly string pass = ConfigurationManager.ConnectionStrings["InfluxPass"].ConnectionString;
        public static string retentionWeek = ConfigurationManager.AppSettings.Get("InfluxRetentionWeek");
        public static string retentionMonth = ConfigurationManager.AppSettings.Get("InfluxRetentionMonth");
        public static string retentionYear = ConfigurationManager.AppSettings.Get("InfluxRetentionYear");

        private readonly InfluxClient databaseClient;
        private Thread writer;

        private bool _isRunning = false;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (value != _isRunning)
                {
                    _isRunning = value;
                    if (value)
                        StartDatabaseWriter();
                }
            }
        }

        public DataManager()
        {
            databaseClient = new InfluxClient(new Uri(serverAddress), user, pass);
            databaseClient.DefaultWriteOptions.Precision = TimestampPrecision.Millisecond;
            databaseClient.DefaultWriteOptions.RetentionPolicy = writeRetention;
            databaseClient.DefaultQueryOptions.Precision = TimestampPrecision.Millisecond;
            Console.WriteLine("0InfluxDB client initialized.");
        }

        private void StartDatabaseWriter()
        {
            int writeCyckles = 0;
            writer = new Thread(async () =>
            {
                while (IsRunning)
                {
                    Thread.Sleep(DbWriteInterval);
                    try
                    {
                        Task writePing = databaseClient.WriteAsync(databaseName, measLat, VerifyRows(PingTransactions));
                        Task writeSig = databaseClient.WriteAsync(databaseName, measSig, VerifyRows(SignalTransactions));
                        Task writeSigQ = databaseClient.WriteAsync(databaseName, measSigQ, VerifyRows(SignalQTransactions));
                        Task writeTx = databaseClient.WriteAsync(databaseName, measTx, VerifyRows(TxTransactions));
                        Task writeRx = databaseClient.WriteAsync(databaseName, measRx, VerifyRows(RxTransactions));
                        Task writeTempOdu = databaseClient.WriteAsync(databaseName, measTmpO, VerifyRows(TempOduTransactions));
                        Task writeTempIdu = databaseClient.WriteAsync(databaseName, measTmpI, VerifyRows(TempIduTransactions));
                        Task writeVoltage = databaseClient.WriteAsync(databaseName, measVolt, VerifyRows(VoltageTransactions));

                        if (++writeCyckles > weatherCycles)
                        {
                            Task writeWeaTemp = databaseClient.WriteAsync(databaseName, measTmpA, VerifyRows(WeatherTempTransactions));
                            Task writeWeather = databaseClient.WriteAsync(databaseName, measWeath, VerifyRows(WeatherOtherTransactions));

                            weatherCycles = 0;
                            await writeWeaTemp;
                            await writeWeather;
                        }

                        await writePing;
                        await writeSig;
                        await writeSigQ;
                        await writeTx;
                        await writeRx;
                        await writeTempOdu;
                        await writeTempIdu;
                        await writeVoltage;
                    }
                    catch (InfluxException e)
                    {
                        if (e.InnerException != null)
                        {
                            if (e.InnerException.InnerException != null)
                                Console.WriteLine(e.InnerException.InnerException.Message);
                            else
                                Console.WriteLine(e.Message);
                        }
                        else
                            Console.WriteLine(e.Message);
                    }
                }
            }) { Name = "influxWriter", Priority = ThreadPriority.AboveNormal };
            writer.Start();
        }

        private List<DynamicInfluxRow> VerifyRows(Queue<DynamicInfluxRow> rows)
        {
            List<DynamicInfluxRow> verified = new List<DynamicInfluxRow>();
            lock (rows)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    DynamicInfluxRow r = rows.Dequeue();
                    if (r != null)
                        verified.Add(r);
                }
            }
            return verified;
        }

        public async Task<List<DynamicInfluxRow>> QueryRows(string query)
        {
            InfluxResultSet<DynamicInfluxRow> resultSet = await databaseClient.ReadAsync<DynamicInfluxRow>(databaseName, query);
            if (resultSet != null)
                if (resultSet.Results.Count > 0)
                    if (resultSet.Results.First().Series.Count > 0)
                        return resultSet.Results.First().Series.First().Rows;
            return null;
        }

        public async Task<List<InfluxSeries<DynamicInfluxRow>>> QuerySeries(string query)
        {
            InfluxResultSet<DynamicInfluxRow> resultSet = await databaseClient.ReadAsync<DynamicInfluxRow>(databaseName, query);
            if (resultSet != null)
                if (resultSet.Results.Count > 0)
                    return resultSet.Results.First().Series;
            return null;
        }

        public async Task<DynamicInfluxRow> QueryValue(string query)
        {
            InfluxResultSet<DynamicInfluxRow> resultSet = await databaseClient.ReadAsync<DynamicInfluxRow>(databaseName, query);
            if (resultSet != null)
                if (resultSet.Results.Count > 0)
                    if (resultSet.Results.First().Series.Count > 0)
                        if (resultSet.Results.First().Series.First().Rows.Count > 0)
                            return resultSet.Results.First().Series.First().Rows.First();
            return null;
        }

        public static long TimeToInfluxTime(DateTime timestamp)
        {
            return ((DateTimeOffset)timestamp.ToUniversalTime()).ToUnixTimeSeconds() * 1000000000;
        }
    }
}
