using System;

namespace MicrowaveMonitor.Database
{
    public abstract class Record
    {
        private DateTime _timeMark;

        public DateTime TimeMark { get => _timeMark; set => _timeMark = value; }

        public Record(DateTime timeMark)
        {
            TimeMark = timeMark;
        }
    }
}
