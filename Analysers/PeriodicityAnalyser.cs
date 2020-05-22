using Itenso.TimePeriod;
using MicrowaveMonitor.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.PeerToPeer.Collaboration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vibrant.InfluxDB.Client;
using Vibrant.InfluxDB.Client.Rows;

namespace MicrowaveMonitor.Analysers
{
    public class PeriodicityAnalyser : Analyser
    {
        private static TimeSpan CalcTenMinInterval = TimeSpan.FromMinutes(10);
        private static TimeSpan CalcHourInterval = TimeSpan.FromMinutes(60);
        private static TimeSpan CalcDayInterval = TimeSpan.FromDays(1);
        private static TimeSpan CalcWeekInterval = TimeSpan.FromDays(7);

        public PeriodicityAnalyser(AlarmManager alarmManager, DataManager dataManager) : base(alarmManager, dataManager)
        {
            RefreshInterval = CalcTenMinInterval;
        }

        public void LoadSettings(PercentDiff percentages)
        {
            Percentages = percentages;
        }

        protected override void Start()
        {
            tQueryer = new Thread(Run) { IsBackground = true, Name = "analyserPeriodicity" };
            tQueryer.Start();
        }

        private void Run()
        {
            while (IsRunning)
            {
                DateTime start = DateTime.Now;



                TimeSpan diff = DateTime.Now - start;
                Thread.Sleep(RefreshInterval - diff);
            }
        }

        private async void Compare(string meas, Dictionary<int, bool> watchInfo, Dictionary<int, int> ids, TimeSpan queryTimeSpan)
        {
            throw new NotImplementedException();
        }

        private void Check()
        {
            throw new NotImplementedException();
        }

        public override void DeviceStopped(int devId)
        {
            throw new NotImplementedException();
        }
    }
}

