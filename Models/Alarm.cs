using SQLite;
using System;

namespace MicrowaveMonitor.Models
{
    public enum AlarmRank
    {
        Info = 1,
        Warning = 2,
        Critical = 3,
        Down = 4
    }

    public enum Measurement
    {
        Traffic = -1,
        All = 0,
        Latency = 1,
        Strength = 2,
        Quality = 3,
        TempODU = 4,
        TempIDU = 5,
        Voltage = 6
    }

    public enum AlarmType
    {
        Down = 0,
        Treshold = 1,
        AvgLong = 2,
        AvgShort = 3,
        Retrospecitve = 4,
        Repetition = 5,
        TempCorrel = 6
    }

    //// bool ValueTrend ///////
    //   -- Below limit = false
    //   -- Over limit = true

    public class Alarm
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public AlarmRank Rank { get; set; } = AlarmRank.Info;
        public bool IsActive { get; set; } = false;
        public bool IsAck { get; set; } = false;
        public bool IsShowed { get; set; } = false;
        public DateTime GenerTime { get; set; }
        public DateTime SettledTime { get; set; }
        [Indexed]
        public int LinkId { get; set; }
        [Indexed]
        public int DeviceId { get; set; }
        public Measurement Measure { get; set; }
        public AlarmType Type { get; set; }
        public bool Trend { get; set; }
        public double GenerValue { get; set; }
        public double SettledValue { get; set; }
        public string DeviceType { get; set; }
    }
}
