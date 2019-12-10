using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicrowaveMonitor.Database
{
    public class UIntRecord
    {
        DateTime _timeMark;
        UInt32 _data;

        public DateTime TimeMark { get => _timeMark; set => _timeMark = value; }
        public uint Data { get => _data; set => _data = value; }

        public UIntRecord(DateTime timeMark, UInt32 data)
        {
            TimeMark = timeMark;
            Data = data;
        }
    }
}
