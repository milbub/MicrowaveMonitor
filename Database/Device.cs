using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Workers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;

namespace MicrowaveMonitor.Database
{
    public class Device : INotifyPropertyChanged
    {
        /* Basic parameters */
        private int _id;
        private IPEndPoint _address;
        private OctetString _communityString;

        public int Id { get => _id; set => _id = value; }
        public IPEndPoint Address { get => _address; set => _address = value; }
        public OctetString CommunityString { get => _communityString; set => _communityString = value; }

        /* Statuses */
        private bool _enableTx = false;
        private bool _enableRx = false;

        public bool IsEnabledTx { get => _enableTx; set => _enableTx = value; }
        public bool IsEnabledRx { get => _enableRx; set => _enableRx = value; }

        /* Model Specific */
        private int _signalQDivider = 10;

        public int SignalQDivider { get => _signalQDivider; set => _signalQDivider = value; }

        /* OIDs */
        private static ObjectIdentifier _oidSysName = new ObjectIdentifier("1.3.6.1.2.1.1.5.0");
        private static ObjectIdentifier _oidUptime = new ObjectIdentifier("1.3.6.1.2.1.1.3.0");
        private ObjectIdentifier _oidSignal;
        private ObjectIdentifier _oidSignalQ;
        private ObjectIdentifier _oidTxDataRate;
        private ObjectIdentifier _oidRxDataRate;

        public static ObjectIdentifier OidSysName { get => _oidSysName; }
        public static ObjectIdentifier OidUptime { get => _oidUptime; }
        public ObjectIdentifier OidSignal { get => _oidSignal; set => _oidSignal = value; }
        public ObjectIdentifier OidSignalQ { get => _oidSignalQ; set => _oidSignalQ = value; }
        public ObjectIdentifier OidTxDataRate { get => _oidTxDataRate; set => _oidTxDataRate = value; }
        public ObjectIdentifier OidRxDataRate { get => _oidRxDataRate; set => _oidRxDataRate = value; }

        /* Refresh interval constants */
        private const int _refreshSysName = 600000;
        private const int _refreshUptime = 10000;
        private int _refreshSignal;
        private int _refreshSignalQ;
        private int _refreshTx;
        private int _refreshRx;
        private int _refreshPing;

        public static int RefreshSysName => _refreshSysName;
        public static int RefreshUptime => _refreshUptime;
        public int RefreshSignal { get => _refreshSignal; set => _refreshSignal = value; }
        public int RefreshSignalQ { get => _refreshSignalQ; set => _refreshSignalQ = value; }
        public int RefreshTx { get => _refreshTx; set => _refreshTx = value; }
        public int RefreshRx { get => _refreshRx; set => _refreshRx = value; }
        public int RefreshPing { get => _refreshPing; set => _refreshPing = value; }

        /* Collected data storages */
        private string _dataSysName;
        private uint _dataUptime;
        private ObservableCollection<RecordDouble> _dataSignal;
        private ObservableCollection<RecordDouble> _dataSignalQ;
        private ObservableCollection<RecordUInt> _dataTx;
        private ObservableCollection<RecordUInt> _dataRx;
        private ObservableCollection<RecordDouble> _dataPing;

        /* Statistic data */
        private double _avgSig, _diffSig = 0;
        private double _avgSigQ, _diffSigQ = 0;
        private double _avgPing, _diffPing = 0;

        public double AvgSig { get => _avgSig; set => _avgSig = value; }
        public double DiffSig { get => _diffSig; set => _diffSig = value; }
        public double AvgSigQ { get => _avgSigQ; set => _avgSigQ = value; }
        public double DiffSigQ { get => _diffSigQ; set => _diffSigQ = value; }
        public double AvgPing { get => _avgPing; set => _avgPing = value; }
        public double DiffPing { get => _diffPing; set => _diffPing = value; }

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
        public uint DataUptime
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
        public ObservableCollection<RecordDouble> DataSignal { get => _dataSignal; set => _dataSignal = value; }
        public ObservableCollection<RecordDouble> DataSignalQ { get => _dataSignalQ; set => _dataSignalQ = value; }
        public ObservableCollection<RecordUInt> DataTx { get => _dataTx; set => _dataTx = value; }
        public ObservableCollection<RecordUInt> DataRx { get => _dataRx; set => _dataRx = value; }
        public ObservableCollection<RecordDouble> DataPing { get => _dataPing; set => _dataPing = value; }


        /* Workers */
        private SnmpSysName _collectorSysName;
        private SnmpUptime _collectorUptime;
        private SnmpSignal _collectorSignal;
        private SnmpSignalQ _collectorSignalQ;
        private SnmpTx _collectorTx;
        private SnmpRx _collectorRx;
        private PingCollector _collectorPing;

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
            DataSignal = new ObservableCollection<RecordDouble>();
            DataSignalQ = new ObservableCollection<RecordDouble>();
            DataTx = new ObservableCollection<RecordUInt>();
            DataRx = new ObservableCollection<RecordUInt>();
            DataPing = new ObservableCollection<RecordDouble>();
        }

        public Device(int id, string ipString, int port, string snmpCommunity, ObjectIdentifier oidSignal, int refreshSig, ObjectIdentifier oidSignalQ, int resfreshSigQ, int refreshPing)
            : this(id, ipString, port, snmpCommunity)
        {
            OidSignal = oidSignal;
            RefreshSignal = refreshSig;
            OidSignalQ = oidSignalQ;
            RefreshSignalQ = resfreshSigQ;
            RefreshPing = refreshPing;
        }

        public Device(int id, string ipString, int port, string snmpCommunity, ObjectIdentifier oidSignal, int refreshSig, ObjectIdentifier oidSignalQ, int resfreshSigQ, ObjectIdentifier oidTx, int refreshTx, ObjectIdentifier oidRx, int refreshRx, int refreshPing)
            : this(id, ipString, port, snmpCommunity, oidSignal, refreshSig, oidSignalQ, resfreshSigQ, refreshPing)
        {
            OidTxDataRate = oidTx;
            RefreshTx = refreshTx;
            OidRxDataRate = oidRx;
            RefreshRx = refreshRx;
            IsEnabledTx = true;
            IsEnabledRx = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
