using System;

namespace MicrowaveMonitor.Database
{
    public class UIntRecord
    {
        private DateTime _timeMark;
        private UInt32 _data;

        public DateTime TimeMark { get => _timeMark; set => _timeMark = value; }
        public uint Data { get => _data; set => _data = value; }

        public UIntRecord(DateTime timeMark, UInt32 data)
        {
            TimeMark = timeMark;
            Data = data;
        }
    }
}
