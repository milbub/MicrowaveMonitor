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

        public List<DynamicInfluxRow> PingTransactions = new List<DynamicInfluxRow>();
        public List<DynamicInfluxRow> SignalTransactions = new List<DynamicInfluxRow>();
        public List<DynamicInfluxRow> SignalQTransactions = new List<DynamicInfluxRow>();
        public List<DynamicInfluxRow> TrafficTransactions = new List<DynamicInfluxRow>();
        public List<DynamicInfluxRow> TempOduTransactions = new List<DynamicInfluxRow>();
        public List<DynamicInfluxRow> TempIduTransactions = new List<DynamicInfluxRow>();
        public List<DynamicInfluxRow> VoltageTransactions = new List<DynamicInfluxRow>();
        public List<DynamicInfluxRow> WeatherTempTransactions = new List<DynamicInfluxRow>();

        private readonly string serverAddress = ConfigurationManager.ConnectionStrings["InfluxServer"].ConnectionString;
        private readonly string databaseName = ConfigurationManager.ConnectionStrings["InfluxData"].ConnectionString;
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
                            VerifyRows(PingTransactions);
                            Task writePing = databaseClient.WriteAsync(databaseName, "ping", PingTransactions);
                            PingTransactions.Clear();
                            await writePing;
                            VerifyRows(SignalTransactions);
                            Task writeSig = databaseClient.WriteAsync(databaseName, "signal", SignalTransactions);
                            SignalTransactions.Clear();
                            await writeSig;
                            VerifyRows(SignalQTransactions);
                            Task writeSigQ = databaseClient.WriteAsync(databaseName, "signalQ", SignalQTransactions);
                            SignalQTransactions.Clear();
                            await writeSigQ;
                            VerifyRows(TrafficTransactions);
                            Task writeTx = databaseClient.WriteAsync(databaseName, "traffic", TrafficTransactions);
                            TrafficTransactions.Clear();
                            await writeTx;
                            VerifyRows(TempOduTransactions);
                            Task writeTempOdu = databaseClient.WriteAsync(databaseName, "tempOdu", TempOduTransactions);
                            TempOduTransactions.Clear();
                            await writeTempOdu;
                            VerifyRows(TempIduTransactions);
                            Task writeTempIdu = databaseClient.WriteAsync(databaseName, "tempIdu", TempIduTransactions);
                            TempIduTransactions.Clear();
                            await writeTempIdu;
                            VerifyRows(VoltageTransactions);
                            Task writeVoltage = databaseClient.WriteAsync(databaseName, "voltage", VoltageTransactions);
                            VoltageTransactions.Clear();
                            await writeVoltage;

                            if (++writeCyckles > weatherCycles)
                            {
                                VerifyRows(WeatherTempTransactions);
                                Task writeWeaTemp = databaseClient.WriteAsync(databaseName, "tempAir", WeatherTempTransactions);
                                WeatherTempTransactions.Clear();
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

        private void VerifyRows(List<DynamicInfluxRow> rows)
        {
            foreach (var row in rows)
            {
                if (row == null)
                    rows.Remove(row);
            }
        }

        public void StopDatabaseWriter()
        {
            IsRunning = false;
        }
    }
}
