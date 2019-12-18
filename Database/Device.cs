using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Workers;
using System.ComponentModel;

namespace MicrowaveMonitor.Database
{
    public class Device : INotifyPropertyChanged
    {
        /* Basic parameters */
        int _id;
        IPEndPoint _address;
        OctetString _communityString;

        public int Id { get => _id; set => _id = value; }
        public IPEndPoint Address { get => _address; set => _address = value; }
        public OctetString CommunityString { get => _communityString; set => _communityString = value; }

        /* OIDs */
        ObjectIdentifier _oidSysName;
        ObjectIdentifier _oidUptime;
        ObjectIdentifier _oidSignal;
        ObjectIdentifier _oidSignalQ;
        ObjectIdentifier _oidTxDataRate;
        ObjectIdentifier _oidRxDataRate;

        public ObjectIdentifier OidSysName { get => _oidSysName; set => _oidSysName = value; }
        public ObjectIdentifier OidUptime { get => _oidUptime; set => _oidUptime = value; }
        public ObjectIdentifier OidSignal { get => _oidSignal; set => _oidSignal = value; }
        public ObjectIdentifier OidSignalQ { get => _oidSignalQ; set => _oidSignalQ = value; }
        public ObjectIdentifier OidTxDataRate { get => _oidTxDataRate; set => _oidTxDataRate = value; }
        public ObjectIdentifier OidRxDataRate { get => _oidRxDataRate; set => _oidRxDataRate = value; }

        /* Refresh interval constants */
        const int _refreshSysName = 600000;
        const int _refreshUptime = 10000;
        int _refreshSignal;
        int _refreshSignalQ;
        int _refreshTx;
        int _refreshRx;
        int _refreshPing;

        public static int RefreshSysName => _refreshSysName;
        public static int RefreshUptime => _refreshUptime;
        public int RefreshSignal { get => _refreshSignal; set => _refreshSignal = value; }
        public int RefreshSignalQ { get => _refreshSignalQ; set => _refreshSignalQ = value; }
        public int RefreshTx { get => _refreshTx; set => _refreshTx = value; }
        public int RefreshRx { get => _refreshRx; set => _refreshRx = value; }
        public int RefreshPing { get => _refreshPing; set => _refreshPing = value; }

        /* Collected data storages */
        string _dataSysName;
        string _dataUptime;
        ObservableCollection<UIntRecord> _dataSignal;
        ObservableCollection<DoubleRecord> _dataSignalQ;
        ObservableCollection<UIntRecord> _dataTx;
        ObservableCollection<UIntRecord> _dataRx;
        ObservableCollection<DoubleRecord> _dataPing;

        public string DataSysName
        {
            get => _dataSysName;
            set
            {
                if (value != _dataSysName)
                {
                    _dataSysName = value;
                    OnPropertyChanged("sysName");
                }
            }
        }
        public string DataUptime
        {
            get => _dataUptime;
            set
            {
                if (value != _dataUptime)
                {
                    _dataUptime = value;
                    OnPropertyChanged("uptime");
                }
            }
        }
        public ObservableCollection<UIntRecord> DataSignal { get => _dataSignal; set => _dataSignal = value; }
        public ObservableCollection<DoubleRecord> DataSignalQ { get => _dataSignalQ; set => _dataSignalQ = value; }
        public ObservableCollection<UIntRecord> DataTx { get => _dataTx; set => _dataTx = value; }
        public ObservableCollection<UIntRecord> DataRx { get => _dataRx; set => _dataRx = value; }
        public ObservableCollection<DoubleRecord> DataPing { get => _dataPing; set => _dataPing = value; }


        /* Workers */
        SnmpSysName _collectorSysName;
        SnmpUptime _collectorUptime;
        SnmpSignal _collectorSignal;
        SnmpSignalQ _collectorSignalQ;
        SnmpTx _collectorTx;
        SnmpRx _collectorRx;
        PingCollector _collectorPing;

        public SnmpSysName CollectorSysName { get => _collectorSysName; set => _collectorSysName = value; }
        public SnmpUptime CollectorUptime { get => _collectorUptime; set => _collectorUptime = value; }
        public SnmpSignal CollectorSignal { get => _collectorSignal; set => _collectorSignal = value; }
        public SnmpSignalQ CollectorSignalQ { get => _collectorSignalQ; set => _collectorSignalQ = value; }
        public SnmpTx CollectorTx { get => _collectorTx; set => _collectorTx = value; }
        public SnmpRx CollectorRx { get => _collectorRx; set => _collectorRx = value; }
        public PingCollector CollectorPing { get => _collectorPing; set => _collectorPing = value; }

        public Device(int id, string ipString, int port, string snmpCommunity)
        {
            Id = id;
            Address = new IPEndPoint(IPAddress.Parse(ipString), port);
            CommunityString = new OctetString(snmpCommunity);
            DataSignal = new ObservableCollection<UIntRecord>();
            DataSignalQ = new ObservableCollection<DoubleRecord>();
            DataTx = new ObservableCollection<UIntRecord>();
            DataRx = new ObservableCollection<UIntRecord>();
            DataPing = new ObservableCollection<DoubleRecord>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
