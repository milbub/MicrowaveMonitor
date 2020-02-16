using System;

namespace MicrowaveMonitor.Database
{
    public class RecordUInt : Record
    {
        private UInt32 _data;

        public uint Data { get => _data; set => _data = value; }

        public RecordUInt(DateTime timeMark, UInt32 data) : base(timeMark)
        {
            Data = data;
        }
    }
}
