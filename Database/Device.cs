using SQLite;

namespace MicrowaveMonitor.Database
{
    public class Device
    {
        public enum SnmpProtocolVersion { v1 = 1, v2 = 2, v3 = 3 }
        private const int defaultRefresh = 1000;

        ///////////////////////// DATABASE /////////////////////////

        /* Basic parameters */
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Address { get; set; }
        public int SnmpPort { get; set; } = 161;
        public SnmpProtocolVersion SnmpVersion { get; set; } = SnmpProtocolVersion.v1;
        public string CommunityString { get; set; } = "public";
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        /* Controls */
        public bool IsPaused { get; set; } = false;
        public bool IsEnabledSignal { get; set; } = true;
        public bool IsEnabledSignalQ { get; set; } = true;
        public bool IsEnabledTx { get; set; } = false;
        public bool IsEnabledRx { get; set; } = false;
        public bool IsEnabledTempOdu { get; set; } = false;
        public bool IsEnabledTempIdu { get; set; } = false;
        public bool IsEnabledVoltage { get; set; } = false;

        /* Model specifics */
        public int SignalQDivisor { get; set; } = 10;

        /* SNMP OIDs */
        public static string OidSysName_s { get; } = "1.3.6.1.2.1.1.5.0";   // RFC 1213
        public static string OidUptime_s { get; } = "1.3.6.1.2.1.1.3.0";    // RFC 1213
        public string OidSignal_s { get; set; }
        public string OidSignalQ_s { get; set; }
        public string OidTxDataRate_s { get; set; }
        public string OidRxDataRate_s { get; set; }
        public string OidTempOdu_s { get; set; }
        public string OidTempIdu_s { get; set; }
        public string OidVoltage_s { get; set; }

        /* Refresh intervals */
        public static int RefreshSysName { get; } = 600000;
        public static int RefreshUptime { get; } = 10000;
        public int RefreshSignal { get; set; } = defaultRefresh;
        public int RefreshSignalQ { get; set; } = defaultRefresh;
        public int RefreshTx { get; set; } = defaultRefresh;
        public int RefreshRx { get; set; } = defaultRefresh;
        public int RefreshPing { get; set; } = defaultRefresh;
        public int RefreshTempOdu { get; set; } = defaultRefresh;
        public int RefreshTempIdu { get; set; } = defaultRefresh;
        public int RefreshVoltage { get; set; } = defaultRefresh;
    }
}
