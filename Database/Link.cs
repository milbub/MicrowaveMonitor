using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicrowaveMonitor.Database
{
    class Link
    {
        byte _hopCount;
        string _name;
        string _note;
        Device _baseDevice;
        Device _endDevice;
        Device _relayOne;
        Device _relayTwo;
        Device _relayThree;
        Device _relayFour;

        public byte HopCount { get => _hopCount; set => _hopCount = value; }
        public string Name { get => _name; set => _name = value; }
        public string Note { get => _note; set => _note = value; }
        internal Device BaseDevice { get => _baseDevice; set => _baseDevice = value; }
        internal Device EndDevice { get => _endDevice; set => _endDevice = value; }
        internal Device RelayOne { get => _relayOne; set => _relayOne = value; }
        internal Device RelayTwo { get => _relayTwo; set => _relayTwo = value; }
        internal Device RelayThree { get => _relayThree; set => _relayThree = value; }
        internal Device RelayFour { get => _relayFour; set => _relayFour = value; }

        public Link(string linkName, Device baseDev)
        {
            _name = linkName;
            _hopCount = 0;
            _baseDevice = baseDev;
        }

        public Link(string linkName, Device baseDev, Device endDev)
        {
            _name = linkName;
            _hopCount = 1;
            _baseDevice = baseDev;
            _endDevice = endDev;
        }
    }
}
