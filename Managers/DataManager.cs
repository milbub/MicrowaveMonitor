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
                        List<DynamicInfluxRow> dataToWrite;
                        Thread.Sleep(dbWriteInterval);
                        try
                        {
                            dataToWrite = PingTransactions.ToList();
                            Task writePing = databaseClient.WriteAsync("MicrowaveMonDB", "ping", dataToWrite);
                            PingTransactions.Clear();
                            await writePing;
                            dataToWrite = SignalTransactions.ToList();
                            Task writeSig = databaseClient.WriteAsync("MicrowaveMonDB", "signal", dataToWrite);
                            SignalTransactions.Clear();
                            await writeSig;
                            dataToWrite = SignalQTransactions.ToList();
                            Task writeSigQ = databaseClient.WriteAsync("MicrowaveMonDB", "signalQ", dataToWrite);
                            SignalQTransactions.Clear();
                            await writeSigQ;
                            dataToWrite = TxTransactions.ToList();
                            Task writeTx = databaseClient.WriteAsync("MicrowaveMonDB", "tx", dataToWrite);
                            TxTransactions.Clear();
                            await writeTx;
                            dataToWrite = RxTransactions.ToList();
                            Task writeRx = databaseClient.WriteAsync("MicrowaveMonDB", "rx", dataToWrite);
                            RxTransactions.Clear();
                            await writeRx;
                            dataToWrite = TempOduTransactions.ToList();
                            Task writeTempOdu = databaseClient.WriteAsync("MicrowaveMonDB", "tempOdu", dataToWrite);
                            TempOduTransactions.Clear();
                            await writeTempOdu;
                            dataToWrite = TempIduTransactions.ToList();
                            Task writeTempIdu = databaseClient.WriteAsync("MicrowaveMonDB", "tempIdu", dataToWrite);
                            TempIduTransactions.Clear();
                            await writeTempIdu;
                            dataToWrite = VoltageTransactions.ToList();
                            Task writeVoltage = databaseClient.WriteAsync("MicrowaveMonDB", "voltage", dataToWrite);
                            VoltageTransactions.Clear();
                            await writeVoltage;

                            if (++writeCyckles > weatherCycles)
                            {
                                dataToWrite = WeatherTempTransactions.ToList();
                                Task writeWeaTemp = databaseClient.WriteAsync("MicrowaveMonDB", "airTemperature", dataToWrite);
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
                        catch (NullReferenceException e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                });
                writer.Start();
            }
        }

        public void StopDatabaseWriter()
        {
            IsRunning = false;
        }
    }
}
