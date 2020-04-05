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
        public static int dbWriteInterval { get; } = 10000;      // 10000 msec
        private static float weatherCycles = (WeatherCollector.MinRefresh * 60000) / dbWriteInterval;

        public const string measLat = "latency";
        public const string measSig = "signal";
        public const string measSigQ = "signalQ";
        public const string measTx = "tx";
        public const string measRx = "rx";
        public const string measTmpO = "tempOdu";
        public const string measTmpI = "tempIdu";
        public const string measVolt = "voltage";
        public const string measTmpA = "tempAir";

        public Queue<DynamicInfluxRow> PingTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> SignalTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> SignalQTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> TxTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> RxTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> TempOduTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> TempIduTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> VoltageTransactions = new Queue<DynamicInfluxRow>();
        public Queue<DynamicInfluxRow> WeatherTempTransactions = new Queue<DynamicInfluxRow>();

        public static string serverAddress = ConfigurationManager.ConnectionStrings["InfluxServer"].ConnectionString;
        public static string databaseName = ConfigurationManager.ConnectionStrings["InfluxData"].ConnectionString;
        public static string retention = ConfigurationManager.ConnectionStrings["InfluxRetention"].ConnectionString;
        private readonly string user = ConfigurationManager.ConnectionStrings["InfluxUser"].ConnectionString;
        private readonly string pass = ConfigurationManager.ConnectionStrings["InfluxPass"].ConnectionString;

        private InfluxClient databaseClient;
        private Thread writer;

        public bool IsRunning { get; set; } = false;

        public DataManager()
        {
            databaseClient = new InfluxClient(new Uri(serverAddress), user, pass);
            databaseClient.DefaultWriteOptions.Precision = TimestampPrecision.Millisecond;
            databaseClient.DefaultQueryOptions.Precision = TimestampPrecision.Millisecond;
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
                        Thread.Sleep(dbWriteInterval);
                        try
                        {
                            //VerifyRows(PingTransactions);
                            Task writePing = databaseClient.WriteAsync(databaseName, measLat, VerifyRows(PingTransactions));
                            //PingTransactions.Clear();
                            await writePing;
                            //VerifyRows(SignalTransactions);
                            Task writeSig = databaseClient.WriteAsync(databaseName, measSig, VerifyRows(SignalTransactions));
                            //SignalTransactions.Clear();
                            await writeSig;
                            //VerifyRows(SignalQTransactions);
                            Task writeSigQ = databaseClient.WriteAsync(databaseName, measSigQ, VerifyRows(SignalQTransactions));
                            //SignalQTransactions.Clear();
                            await writeSigQ;
                            //VerifyRows(TxTransactions);
                            Task writeTx = databaseClient.WriteAsync(databaseName, measTx, VerifyRows(TxTransactions));
                            //TxTransactions.Clear();
                            await writeTx;
                            //VerifyRows(RxTransactions);
                            Task writeRx = databaseClient.WriteAsync(databaseName, measRx, VerifyRows(RxTransactions));
                            //RxTransactions.Clear();
                            await writeRx;
                            //VerifyRows(TempOduTransactions);
                            Task writeTempOdu = databaseClient.WriteAsync(databaseName, measTmpO, VerifyRows(TempOduTransactions));
                            //TempOduTransactions.Clear();
                            await writeTempOdu;
                            //VerifyRows(TempIduTransactions);
                            Task writeTempIdu = databaseClient.WriteAsync(databaseName, measTmpI, VerifyRows(TempIduTransactions));
                            //TempIduTransactions.Clear();
                            await writeTempIdu;
                            //VerifyRows(VoltageTransactions);
                            Task writeVoltage = databaseClient.WriteAsync(databaseName, measVolt, VerifyRows(VoltageTransactions));
                            //VoltageTransactions.Clear();
                            await writeVoltage;

                            if (++writeCyckles > weatherCycles)
                            {
                                //VerifyRows(WeatherTempTransactions);
                                Task writeWeaTemp = databaseClient.WriteAsync(databaseName, measTmpA, VerifyRows(WeatherTempTransactions));
                                //WeatherTempTransactions.Clear();
                                weatherCycles = 0;
                                await writeWeaTemp;
                            }
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
                        catch (InvalidOperationException e)
                        {
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
            for (int i = 0; i < rows.Count; i++)
            {
                DynamicInfluxRow r = rows.Dequeue();
                if (r != null)
                    verified.Add(r);
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

        public async Task<object> QueryValue(string query)
        {
            InfluxResultSet<DynamicInfluxRow> resultSet = await databaseClient.ReadAsync<DynamicInfluxRow>(databaseName, query);
            return resultSet.Results[0].Series[0].Rows[0].Fields.Values.First();
        }

        public void StopDatabaseWriter()
        {
            IsRunning = false;
        }
    }
}
