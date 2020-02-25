using Lextm.SharpSnmpLib;
using MicrowaveMonitor.Workers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using SQLite;

namespace MicrowaveMonitor.Database
{
    public class Device : INotifyPropertyChanged
    {
        ///////////////////////// DATABASE /////////////////////////

        /* Basic parameters */
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public int SnmpPort { get; set; } = 161;
        public string CommunityString { get; set; } = "public";

        /* Statuses */
        public bool IsEnabledTx { get; set; } = false;
        public bool IsEnabledRx { get; set; } = false;

        /* Model specific */
        public int SignalQDivider { get; set; } = 10;

        /* SNMP OIDs */
        public static string OidSysName_s = "1.3.6.1.2.1.1.5.0";
        public static string OidUptime_s = "1.3.6.1.2.1.1.3.0";
        public string OidSignal_s { get; set; }
        public string OidSignalQ_s { get; set; }
        public string OidTxDataRate_s { get; set; }
        public string OidRxDataRate_s { get; set; }

        /* Refresh intervals */
        public static int RefreshSysName { get; } = 600000;
        public static int RefreshUptime { get; } = 10000;
        public int RefreshSignal { get; set; } = 1000;
        public int RefreshSignalQ { get; set; } = 1000;
        public int RefreshTx { get; set; } = 1000;
        public int RefreshRx { get; set; } = 1000;
        public int RefreshPing { get; set; } = 1000;

        ///////////////////////// CONVERTORS /////////////////////////

        [Ignore]
        public IPAddress Address { get => IPAddress.Parse(IpAddress); set { IpAddress = value.ToString(); } }
        [Ignore]
        public static ObjectIdentifier OidSysName { get => new ObjectIdentifier(OidSysName_s); }
        [Ignore]
        public static ObjectIdentifier OidUptime { get => new ObjectIdentifier(OidUptime_s); }
        [Ignore]
        public ObjectIdentifier OidSignal { get => new ObjectIdentifier(OidSignal_s); set { OidSignal_s = value.ToString(); } }
        [Ignore]
        public ObjectIdentifier OidSignalQ { get => new ObjectIdentifier(OidSignalQ_s); set { OidSignalQ_s = value.ToString(); } }
        [Ignore]
        public ObjectIdentifier OidTxDataRate { get => new ObjectIdentifier(OidTxDataRate_s); set { OidTxDataRate_s = value.ToString(); } }
        [Ignore]
        public ObjectIdentifier OidRxDataRate { get => new ObjectIdentifier(OidRxDataRate_s); set { OidRxDataRate_s = value.ToString(); } }

        ///////////////////////// DATA STORAGES /////////////////////////

        /* Statistical data */
        [Ignore]
        public double AvgSig { get; set; } = 0;
        [Ignore]
        public double DiffSig { get; set; } = 0;
        [Ignore]
        public double AvgSigQ { get; set; } = 0;
        [Ignore]
        public double DiffSigQ { get; set; } = 0;
        [Ignore]
        public double AvgPing { get; set; } = 0;
        [Ignore]
        public double DiffPing { get; set; } = 0;

        /* Collected data */
        [Ignore]
        public ObservableCollection<RecordDouble> DataSignal { get; set; } = new ObservableCollection<RecordDouble>();
        [Ignore]
        public ObservableCollection<RecordDouble> DataSignalQ { get; set; } = new ObservableCollection<RecordDouble>();
        [Ignore]
        public ObservableCollection<RecordUInt> DataTx { get; set; } = new ObservableCollection<RecordUInt>();
        [Ignore]
        public ObservableCollection<RecordUInt> DataRx { get; set; } = new ObservableCollection<RecordUInt>();
        [Ignore]
        public ObservableCollection<RecordDouble> DataPing { get; set; } = new ObservableCollection<RecordDouble>();

        private string _dataSysName;
        private uint _dataUptime;

        [Ignore]
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
        [Ignore]
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

        /* Workers */
        [Ignore]
        public SnmpSysName CollectorSysName { get; set; }
        [Ignore]
        public SnmpUptime CollectorUptime { get; set; }
        [Ignore]
        public SnmpSignal CollectorSignal { get; set; }
        [Ignore]
        public SnmpSignalQ CollectorSignalQ { get; set; }
        [Ignore]
        public SnmpTx CollectorTx { get; set; }
        [Ignore]
        public SnmpRx CollectorRx { get; set; }
        [Ignore]
        public PingCollector CollectorPing { get; set; }

        //public Device(int id, string ipString, int port, string snmpCommunity)
        //{
        //    Id = id;
        //    Address = new IPEndPoint(IPAddress.Parse(ipString), port);
        //    CommunityString = new OctetString(snmpCommunity);
        //    DataSignal = new ObservableCollection<RecordDouble>();
        //    DataSignalQ = new ObservableCollection<RecordDouble>();
        //    DataTx = new ObservableCollection<RecordUInt>();
        //    DataRx = new ObservableCollection<RecordUInt>();
        //    DataPing = new ObservableCollection<RecordDouble>();
        //}

        //public Device(int id, string ipString, int port, string snmpCommunity, ObjectIdentifier oidSignal, int refreshSig, ObjectIdentifier oidSignalQ, int resfreshSigQ, int refreshPing)
        //    : this(id, ipString, port, snmpCommunity)
        //{
        //    OidSignal = oidSignal;
        //    RefreshSignal = refreshSig;
        //    OidSignalQ = oidSignalQ;
        //    RefreshSignalQ = resfreshSigQ;
        //    RefreshPing = refreshPing;
        //}

        //public Device(int id, string ipString, int port, string snmpCommunity, ObjectIdentifier oidSignal, int refreshSig, ObjectIdentifier oidSignalQ, int resfreshSigQ, ObjectIdentifier oidTx, int refreshTx, ObjectIdentifier oidRx, int refreshRx, int refreshPing)
        //    : this(id, ipString, port, snmpCommunity, oidSignal, refreshSig, oidSignalQ, resfreshSigQ, refreshPing)
        //{
        //    OidTxDataRate = oidTx;
        //    RefreshTx = refreshTx;
        //    OidRxDataRate = oidRx;
        //    RefreshRx = refreshRx;
        //    IsEnabledTx = true;
        //    IsEnabledRx = true;
        //}

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
