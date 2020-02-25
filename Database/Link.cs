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
        //internal Device BaseDevice { get; set; }
        //internal Device EndDevice { get; set; }
        //internal Device RelayOne { get; set; }
        //internal Device RelayTwo { get; set; }
        //internal Device RelayThree { get; set; }
        //internal Device RelayFour { get; set; }

        //public Link(string linkName, int id, Device baseDev)
        //{
        //    Name = linkName;
        //    Id = id;
        //    HopCount = 0;
        //    BaseDevice = baseDev;
        //}

        //public Link(string linkName, int id, Device baseDev, Device endDev) : this(linkName, id, baseDev)
        //{
        //    EndDevice = endDev;
        //}

        //public Link(string linkName, int id, Device baseDev, Device endDev, Device relayOne) : this(linkName, id, baseDev, endDev)
        //{
        //    RelayOne = relayOne;
        //}

        //public Link(string linkName, int id, Device baseDev, Device endDev, Device relayOne, Device relayTwo) : this(linkName, id, baseDev, endDev, relayOne)
        //{
        //    RelayTwo = relayTwo;
        //}

        //public Link(string linkName, int id, Device baseDev, Device endDev, Device relayOne, Device relayTwo, Device relayThree) : this(linkName, id, baseDev, endDev, relayOne, relayTwo)
        //{
        //    RelayThree = relayThree;
        //}

        //public Link(string linkName, int id, Device baseDev, Device endDev, Device relayOne, Device relayTwo, Device relayThree, Device relayFour) : this(linkName, id, baseDev, endDev, relayOne, relayTwo, relayThree)
        //{
        //    RelayFour = relayFour;
        //}
    }
}
