using System;

namespace MicrowaveMonitor.Database
{
    public class DoubleRecord
    {
        private DateTime _timeMark;
        private double _data;

        public DateTime TimeMark { get => _timeMark; set => _timeMark = value; }
        public double Data { get => _data; set => _data = value; }

        public DoubleRecord(DateTime timeMark, double data)
        {
            TimeMark = timeMark;
            Data = data;
        }
    }
}
