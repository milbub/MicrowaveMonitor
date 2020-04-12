using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MicrowaveMonitor.Workers;
using Vibrant.InfluxDB.Client;
using Vibrant.InfluxDB.Client.Rows;

namespace MicrowaveMonitor.Managers
{
    public class DataManager
    {
        public static int DbWriteInterval { get; } = 10000;      // 10000 msec
        private static float weatherCycles = (WeatherCollector.MinRefresh * 60000) / DbWriteInterval;

        public readonly string measLat = "latency";
        public readonly string measSig = "signal";
        public readonly string measSigQ = "signalQ";
        public readonly string measTx = "tx";
        public readonly string measRx = "rx";
        public readonly string measTmpO = "tempOdu";
        public readonly string measTmpI = "tempIdu";
        public readonly string measVolt = "voltage";
        public readonly string measTmpA = "tempAir";

        public Queue<DynamicInfluxRow> PingTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> SignalTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> SignalQTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> TxTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> RxTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> TempOduTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> TempIduTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> VoltageTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> WeatherTempTransactions = new Queue<DynamicInfluxRow>();

        public readonly string serverAddress = ConfigurationManager.ConnectionStrings["InfluxServer"].ConnectionString;
        public readonly string databaseName = ConfigurationManager.ConnectionStrings["InfluxData"].ConnectionString;
        public readonly string retention = ConfigurationManager.ConnectionStrings["InfluxRetention"].ConnectionString;
        private readonly string user = ConfigurationManager.ConnectionStrings["InfluxUser"].ConnectionString;
        private readonly string pass = ConfigurationManager.ConnectionStrings["InfluxPass"].ConnectionString;

        private readonly InfluxClient databaseClient;
        private Thread writer;

        public bool IsRunning { get; set; } = false;

        public DataManager()
        {
            databaseClient = new InfluxClient(new Uri(serverAddress), user, pass);
            databaseClient.DefaultWriteOptions.Precision = TimestampPrecision.Millisecond;
            databaseClient.DefaultQueryOptions.Precision = TimestampPrecision.Millisecond;
            Console.WriteLine("0InfluxDB client initialized.");
        }

        public void StartDatabaseWriter()
        {
            if (IsRunning == false)
            {
                int writeCyckles = 0;
                IsRunning = true;
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
                                weatherCycles = 0;
                                await writeWeaTemp;
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
                            } else
                                Console.WriteLine(e.Message);
                        }
                    }
                });
                writer.Start();
            }
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

        public async Task<InfluxSeries<DynamicInfluxRow>> QuerySeries(string query)
        {
            InfluxResultSet<DynamicInfluxRow> resultSet = await databaseClient.ReadAsync<DynamicInfluxRow>(databaseName, query);
            if (resultSet != null)
                if (resultSet.Results.First().Series.Count > 0)
                    return resultSet.Results.First().Series.First();
            return null;
        }

        public async Task<DynamicInfluxRow> QueryValue(string query)
        {
            InfluxResultSet<DynamicInfluxRow> resultSet = await databaseClient.ReadAsync<DynamicInfluxRow>(databaseName, query);
            if (resultSet != null)
                if (resultSet.Results.First().Series.Count > 0)
                    if (resultSet.Results.First().Series.First().Rows.Count > 0)
                        return resultSet.Results.First().Series.First().Rows.First();
            return null;
        }

        public void StopDatabaseWriter()
        {
            IsRunning = false;
        }
    }
}
