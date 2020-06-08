using SQLite;

namespace MicrowaveMonitor.Models
{
    public class Link
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public byte HopCount { get; set; } = 0;
        public string Name { get; set; }
        public string Note { get; set; }
        [Indexed]
        public int DeviceBaseId { get; set; } = 0;
        [Indexed]
        public int DeviceEndId { get; set; } = 0;
        [Indexed]
        public int DeviceR1Id { get; set; } = 0;
        [Indexed]
        public int DeviceR2Id { get; set; } = 0;
        [Indexed]
        public int DeviceR3Id { get; set; } = 0;
        [Indexed]
        public int DeviceR4Id { get; set; } = 0;
    }
}
