using SQLite;

namespace MicrowaveMonitor.Database
{
    public class Link
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public byte HopCount { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        [Indexed]
        public int DeviceBaseId { get; set; }
        [Indexed]
        public int DeviceEndId { get; set; }
        [Indexed]
        public int DeviceR1Id { get; set; }
        [Indexed]
        public int DeviceR2Id { get; set; }
        [Indexed]
        public int DeviceR3Id { get; set; }
        [Indexed]
        public int DeviceR4Id { get; set; }
    }
}
