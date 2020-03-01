using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client;
using Vibrant.InfluxDB.Client.Rows;

namespace MicrowaveMonitor.Managers
{
    public class DataManager
    {
        public List<DynamicInfluxRow> PingTransactions = new List<DynamicInfluxRow>();
        public List<DynamicInfluxRow> SignalTransactions = new List<DynamicInfluxRow>();
        public List<DynamicInfluxRow> SignalQTransactions = new List<DynamicInfluxRow>();
        public List<DynamicInfluxRow> TxTransactions = new List<DynamicInfluxRow>();
        public List<DynamicInfluxRow> RxTransactions = new List<DynamicInfluxRow>();

        private InfluxClient databaseClient = new InfluxClient(new Uri(ConfigurationManager.ConnectionStrings["InfluxData"].ConnectionString));
        private Thread writer;

        public bool IsRunning { get; set; } = false;

        public void StartDatabaseWriter()
        {
            if (IsRunning == false)
            {
                IsRunning = true;
                writer = new Thread(async () =>
                {
                    while (IsRunning)
                    {
                        Thread.Sleep(10000);
                        try
                        {
                            Task writePing = databaseClient.WriteAsync("MicrowaveMonDB", "ping", PingTransactions);
                            PingTransactions.Clear();
                            await writePing;
                            Task writeSig = databaseClient.WriteAsync("MicrowaveMonDB", "signal", SignalTransactions);
                            SignalTransactions.Clear();
                            await writeSig;
                            Task writeSigQ = databaseClient.WriteAsync("MicrowaveMonDB", "signalQ", SignalQTransactions);
                            SignalQTransactions.Clear();
                            await writeSigQ;
                            Task writeTx = databaseClient.WriteAsync("MicrowaveMonDB", "tx", TxTransactions);
                            TxTransactions.Clear();
                            await writeTx;
                            Task writeRx = databaseClient.WriteAsync("MicrowaveMonDB", "rx", RxTransactions);
                            RxTransactions.Clear();
                            await writeRx;
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

        public void StopDatabaseWriter()
        {
            IsRunning = false;
        }
    }
}
