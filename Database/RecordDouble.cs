using System;

namespace MicrowaveMonitor.Database
{
    public class RecordDouble : Record
    {
        private double _data;

        public double Data { get => _data; set => _data = value; }

        public RecordDouble(DateTime timeMark, double data) : base(timeMark)
        {
            Data = data;
        }
    }
}
