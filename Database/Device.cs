using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Lextm.SharpSnmpLib;

namespace MicrowaveMonitor.Database
{
    public class Device
    {
        int _id;
        IPEndPoint _address;
        OctetString _communityString;
        
        ObjectIdentifier _oidUptime;
        ObjectIdentifier _oidSignalLevel;
        ObjectIdentifier _oidSignalQuality;
        ObjectIdentifier _oidTxDataRate;
        ObjectIdentifier _oidRxDataRate;

        const UInt32 _uptimeRefresh = 10000;
        UInt32 _signalLevelRefresh;
        UInt32 _signalQualityRefresh;
        UInt32 _txRefresh;
        UInt32 _rxRefresh;

        string _dataUptime;
        ObservableCollection<UIntRecord> _dataSignalLevel;
        ObservableCollection<UIntRecord> _dataSignalQuality;
        ObservableCollection<UIntRecord> _dataTx;
        ObservableCollection<UIntRecord> _dataRx;

        public int Id { get => _id; set => _id = value; }
        public IPEndPoint Address { get => _address; set => _address = value; }
        public OctetString CommunityString { get => _communityString; set => _communityString = value; }
        public ObjectIdentifier OidUptime { get => _oidUptime; set => _oidUptime = value; }
        public ObjectIdentifier OidSignalLevel { get => _oidSignalLevel; set => _oidSignalLevel = value; }
        public ObjectIdentifier OidSignalQuality { get => _oidSignalQuality; set => _oidSignalQuality = value; }
        public ObjectIdentifier OidTxDataRate { get => _oidTxDataRate; set => _oidTxDataRate = value; }
        public ObjectIdentifier OidRxDataRate { get => _oidRxDataRate; set => _oidRxDataRate = value; }
        public uint SignalLevelRefresh { get => _signalLevelRefresh; set => _signalLevelRefresh = value; }
        public uint SignalQualityRefresh { get => _signalQualityRefresh; set => _signalQualityRefresh = value; }
        public uint TxRefresh { get => _txRefresh; set => _txRefresh = value; }
        public uint RxRefresh { get => _rxRefresh; set => _rxRefresh = value; }
        public string DataUptime { get => _dataUptime; set => _dataUptime = value; }
        public ObservableCollection<UIntRecord> DataSignalLevel { get => _dataSignalLevel; set => _dataSignalLevel = value; }
        public ObservableCollection<UIntRecord> DataSignalQuality { get => _dataSignalQuality; set => _dataSignalQuality = value; }
        public ObservableCollection<UIntRecord> DataTx { get => _dataTx; set => _dataTx = value; }
        public ObservableCollection<UIntRecord> DataRx { get => _dataRx; set => _dataRx = value; }
        public static uint UptimeRefresh => _uptimeRefresh;

        public Device(int id, string ipString, int port, string snmpCommunity)
        {
            Id = id;
            Address = new IPEndPoint(IPAddress.Parse(ipString), port);
            CommunityString = new OctetString(snmpCommunity);
            DataSignalLevel = new ObservableCollection<UIntRecord>();
            DataSignalQuality = new ObservableCollection<UIntRecord>();
            DataTx = new ObservableCollection<UIntRecord>();
            DataRx = new ObservableCollection<UIntRecord>();
        }
    }
}
