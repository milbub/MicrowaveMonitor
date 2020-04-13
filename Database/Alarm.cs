using SQLite;
using System;

namespace MicrowaveMonitor.Database
{
    public enum AlarmRank
    {
        Info = 0,
        Warning = 1,
        Critical = 2,
        Down = 3
    }

    public enum Measurement
    {
        All = 0,
        Ping = 1,
        Signal = 2,
        SignalQ = 3,
        TempOdu = 4,
        TempIdu = 5,
        Voltage = 6
    }

    public enum AlarmType
    {
        Down = 0,
        Treshold = 1,
        AvgLong = 2,
        AvgShort = 3,
        Longterm = 4,
        Repetition = 5,
        TempCorrel = 6
    }

    public enum ValueTrend
    {
        Below = 0,
        Over = 1
    }

    public class Alarm
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public AlarmRank Rank { get; set; } = AlarmRank.Info;
        public bool IsActive { get; set; } = false;
        public bool IsAck { get; set; } = false;
        public DateTime GenerTime { get; set; }
        public DateTime SettledTime { get; set; }
        [Indexed]
        public int LinkId { get; set; }
        [Indexed]
        public int DeviceId { get; set; }
        public Measurement Measure { get; set; }
        public AlarmType Type { get; set; }
        public ValueTrend Trend { get; set; }
        public double GenerValue { get; set; }
        public double SettledValue { get; set; }
    }
}
