using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicrowaveMonitor.Database
{
    public class DoubleRecord
    {
        DateTime _timeMark;
        double _data;

        public DateTime TimeMark { get => _timeMark; set => _timeMark = value; }
        public double Data { get => _data; set => _data = value; }

        public DoubleRecord(DateTime timeMark, double data)
        {
            TimeMark = timeMark;
            Data = data;
        }
    }
}
