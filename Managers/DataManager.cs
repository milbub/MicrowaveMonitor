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
        public List<DynamicInfluxRow> TxTransactions = new List<DynamicInfluxRow>();
        public List<DynamicInfluxRow> RxTransactions = new List<DynamicInfluxRow>();
        public List<DynamicInfluxRow> TempOduTransactions = new List<DynamicInfluxRow>();
        public List<DynamicInfluxRow> TempIduTransactions = new List<DynamicInfluxRow>();
        public List<DynamicInfluxRow> VoltageTransactions = new List<DynamicInfluxRow>();
        public List<DynamicInfluxRow> WeatherTempTransactions = new List<DynamicInfluxRow>();

        private InfluxClient databaseClient = new InfluxClient(new Uri(ConfigurationManager.ConnectionStrings["InfluxData"].ConnectionString));
        private Thread writer;

        public bool IsRunning { get; set; } = false;

        public DataManager()
        {
            databaseClient.DefaultWriteOptions.Precision = TimestampPrecision.Millisecond;
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
                            Task writePing = databaseClient.WriteAsync("MicrowaveMonDB", "ping", PingTransactions);
                            PingTransactions.Clear();
                            await writePing;
                            VerifyRows(SignalTransactions);
                            Task writeSig = databaseClient.WriteAsync("MicrowaveMonDB", "signal", SignalTransactions);
                            SignalTransactions.Clear();
                            await writeSig;
                            VerifyRows(SignalQTransactions);
                            Task writeSigQ = databaseClient.WriteAsync("MicrowaveMonDB", "signalQ", SignalQTransactions);
                            SignalQTransactions.Clear();
                            await writeSigQ;
                            VerifyRows(TxTransactions);
                            Task writeTx = databaseClient.WriteAsync("MicrowaveMonDB", "tx", TxTransactions);
                            TxTransactions.Clear();
                            await writeTx;
                            VerifyRows(RxTransactions);
                            Task writeRx = databaseClient.WriteAsync("MicrowaveMonDB", "rx", RxTransactions);
                            RxTransactions.Clear();
                            await writeRx;
                            VerifyRows(TempOduTransactions);
                            Task writeTempOdu = databaseClient.WriteAsync("MicrowaveMonDB", "tempOdu", TempOduTransactions);
                            TempOduTransactions.Clear();
                            await writeTempOdu;
                            VerifyRows(TempIduTransactions);
                            Task writeTempIdu = databaseClient.WriteAsync("MicrowaveMonDB", "tempIdu", TempIduTransactions);
                            TempIduTransactions.Clear();
                            await writeTempIdu;
                            VerifyRows(VoltageTransactions);
                            Task writeVoltage = databaseClient.WriteAsync("MicrowaveMonDB", "voltage", VoltageTransactions);
                            VoltageTransactions.Clear();
                            await writeVoltage;

                            if (++writeCyckles > weatherCycles)
                            {
                                VerifyRows(WeatherTempTransactions);
                                Task writeWeaTemp = databaseClient.WriteAsync("MicrowaveMonDB", "airTemperature", WeatherTempTransactions);
                                WeatherTempTransactions.Clear();
                                weatherCycles = 0;
                                await writeWeaTemp;
                            }
                        }
                        catch (InfluxException e)
                        {
                            if (e.InnerException.InnerException != null)
                                Console.WriteLine(e.InnerException.InnerException.Message);
                            else
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

        public void VerifyRows(List<DynamicInfluxRow> rows)
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
