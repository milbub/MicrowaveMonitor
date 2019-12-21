namespace MicrowaveMonitor.Database
{
    public class Link
    {
        private int _id;
        private byte _hopCount;
        private string _name;
        private string _note;
        private Device _baseDevice;
        private Device _endDevice;
        private Device _relayOne;
        private Device _relayTwo;
        private Device _relayThree;
        private Device _relayFour;

        public int Id { get => _id; set => _id = value; }
        public byte HopCount { get => _hopCount; set => _hopCount = value; }
        public string Name { get => _name; set => _name = value; }
        public string Note { get => _note; set => _note = value; }
        internal Device BaseDevice { get => _baseDevice; set => _baseDevice = value; }
        internal Device EndDevice { get => _endDevice; set => _endDevice = value; }
        internal Device RelayOne { get => _relayOne; set => _relayOne = value; }
        internal Device RelayTwo { get => _relayTwo; set => _relayTwo = value; }
        internal Device RelayThree { get => _relayThree; set => _relayThree = value; }
        internal Device RelayFour { get => _relayFour; set => _relayFour = value; }

        public Link(string linkName, int id, Device baseDev)
        {
            Name = linkName;
            Id = id;
            HopCount = 0;
            BaseDevice = baseDev;
        }

        public Link(string linkName, int id, Device baseDev, Device endDev)
        {
            Name = linkName;
            Id = id;
            HopCount = 1;
            BaseDevice = baseDev;
            EndDevice = endDev;
        }

        public Link(string linkName, int id, Device baseDev, Device endDev, Device relayOne)
        {
            Name = linkName;
            Id = id;
            HopCount = 1;
            BaseDevice = baseDev;
            EndDevice = endDev;
            RelayOne = relayOne;
        }

        public Link(string linkName, int id, Device baseDev, Device endDev, Device relayOne, Device relayTwo)
        {
            Name = linkName;
            Id = id;
            HopCount = 1;
            BaseDevice = baseDev;
            EndDevice = endDev;
            RelayOne = relayOne;
            RelayTwo = relayTwo;
        }

        public Link(string linkName, int id, Device baseDev, Device endDev, Device relayOne, Device relayTwo, Device relayThree)
        {
            Name = linkName;
            Id = id;
            HopCount = 1;
            BaseDevice = baseDev;
            EndDevice = endDev;
            RelayOne = relayOne;
            RelayTwo = relayTwo;
            RelayThree = relayThree;
        }

        public Link(string linkName, int id, Device baseDev, Device endDev, Device relayOne, Device relayTwo, Device relayThree, Device relayFour)
        {
            Name = linkName;
            Id = id;
            HopCount = 1;
            BaseDevice = baseDev;
            EndDevice = endDev;
            RelayOne = relayOne;
            RelayTwo = relayTwo;
            RelayThree = relayThree;
            RelayFour = relayFour;
        }
    }
}
