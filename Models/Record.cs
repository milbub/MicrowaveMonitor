using System;

namespace MicrowaveMonitor.Models
{
    public class Record<T>
    {
        public DateTime TimeMark { get; set; }
        public T Data { get; set; }

        public Record(DateTime timeMark, T data)
        {
            TimeMark = timeMark;
            Data = data;
        }
    }
}
