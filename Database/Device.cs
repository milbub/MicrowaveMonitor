using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Workers;

namespace MicrowaveMonitor.Database
{
    public class Device
    {
        /* Basic parameters */
        int _id;
        IPEndPoint _address;
        OctetString _communityString;
        
        /* OIDs */
        ObjectIdentifier _oidUptime;
        ObjectIdentifier _oidSignal;
        ObjectIdentifier _oidSignalQ;
        ObjectIdentifier _oidTxDataRate;
        ObjectIdentifier _oidRxDataRate;

        /* Refresh interval constants */
        const uint _refreshUptime = 10000;
        uint _refreshSignal;
        uint _refreshSignalQ;
        uint _refreshTx;
        uint _refreshRx;

        /* Collected data storages */
        string _dataUptime;
        ObservableCollection<UIntRecord> _dataSignal;
        ObservableCollection<DoubleRecord> _dataSignalQ;
        ObservableCollection<UIntRecord> _dataTx;
        ObservableCollection<UIntRecord> _dataRx;

        /* Workers */
        SnmpUptime _collectorUptime;
        SnmpSignal _collectorSignal;
        SnmpSignalQ _collectorSignalQ;
        SnmpTx _collectorTx;
        SnmpRx _collectorRx;

        public int Id { get => _id; set => _id = value; }
        public IPEndPoint Address { get => _address; set => _address = value; }
        public OctetString CommunityString { get => _communityString; set => _communityString = value; }
        public ObjectIdentifier OidUptime { get => _oidUptime; set => _oidUptime = value; }
        public ObjectIdentifier OidSignal { get => _oidSignal; set => _oidSignal = value; }
        public ObjectIdentifier OidSignalQ { get => _oidSignalQ; set => _oidSignalQ = value; }
        public ObjectIdentifier OidTxDataRate { get => _oidTxDataRate; set => _oidTxDataRate = value; }
        public ObjectIdentifier OidRxDataRate { get => _oidRxDataRate; set => _oidRxDataRate = value; }
        public static uint RefreshUptime => _refreshUptime;
        public uint RefreshSignal { get => _refreshSignal; set => _refreshSignal = value; }
        public uint RefreshSignalQ { get => _refreshSignalQ; set => _refreshSignalQ = value; }
        public uint RefreshTx { get => _refreshTx; set => _refreshTx = value; }
        public uint RefreshRx { get => _refreshRx; set => _refreshRx = value; }
        public string DataUptime { get => _dataUptime; set => _dataUptime = value; }
        public ObservableCollection<UIntRecord> DataSignal { get => _dataSignal; set => _dataSignal = value; }
        public ObservableCollection<DoubleRecord> DataSignalQ { get => _dataSignalQ; set => _dataSignalQ = value; }
        public ObservableCollection<UIntRecord> DataTx { get => _dataTx; set => _dataTx = value; }
        public ObservableCollection<UIntRecord> DataRx { get => _dataRx; set => _dataRx = value; }
        public SnmpUptime CollectorUptime { get => _collectorUptime; set => _collectorUptime = value; }
        public SnmpSignal CollectorSignal { get => _collectorSignal; set => _collectorSignal = value; }
        public SnmpSignalQ CollectorSignalQ { get => _collectorSignalQ; set => _collectorSignalQ = value; }
        public SnmpTx CollectorTx { get => _collectorTx; set => _collectorTx = value; }
        public SnmpRx CollectorRx { get => _collectorRx; set => _collectorRx = value; }

        public Device(int id, string ipString, int port, string snmpCommunity)
        {
            Id = id;
            Address = new IPEndPoint(IPAddress.Parse(ipString), port);
            CommunityString = new OctetString(snmpCommunity);
            DataSignal = new ObservableCollection<UIntRecord>();
            DataSignalQ = new ObservableCollection<DoubleRecord>();
            DataTx = new ObservableCollection<UIntRecord>();
            DataRx = new ObservableCollection<UIntRecord>();
        }
    }
}
