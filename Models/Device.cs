using SQLite;

namespace MicrowaveMonitor.Models
{
    public class Device
    {
        public enum SnmpProtocolVersion { v1 = 1, v2 = 2, v3 = 3 }
        private const int defaultRefresh = 1000;

        ///////////////////////// DATABASE /////////////////////////

        /* Basic parameters */
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Address { get; set; } = "0.0.0.0";
        public int SnmpPort { get; set; } = 161;
        public SnmpProtocolVersion SnmpVersion { get; set; } = SnmpProtocolVersion.v1;
        public string CommunityString { get; set; } = "public";
        public string Latitude { get; set; } = "0";
        public string Longitude { get; set; } = "0";

        /* Controls */
        public bool IsPaused { get; set; } = false;

        public bool IsEnabledSignal { get; set; } = false;
        public bool IsEnabledSignalQ { get; set; } = false;
        public bool IsEnabledTx { get; set; } = false;
        public bool IsEnabledRx { get; set; } = false;
        public bool IsEnabledTempOdu { get; set; } = false;
        public bool IsEnabledTempIdu { get; set; } = false;
        public bool IsEnabledVoltage { get; set; } = false;

        public bool IsWatchedSignal { get; set; } = false;
        public bool IsWatchedSignalQ { get; set; } = false;
        public bool IsWatchedTempOdu { get; set; } = false;
        public bool IsWatchedTempIdu { get; set; } = false;
        public bool IsWatchedVoltage { get; set; } = false;
        public bool IsWatchedPing { get; set; } = false;

        public bool TresholdSignal { get; set; } = false;
        public bool TresholdSignalQ { get; set; } = false;
        public bool TresholdTx { get; set; } = false;
        public bool TresholdRx { get; set; } = false;
        public bool TresholdTempOdu { get; set; } = false;
        public bool TresholdTempIdu { get; set; } = false;
        public bool TresholdVoltage { get; set; } = false;
        public bool TresholdPing { get; set; } = false;

        /* Model specifics */
        public int SignalQDivisor { get; set; } = 10;
        public bool IsTempIduOutdoor { get; set; } = false;

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

        /* Tresholds */
        public float TreshUpSignal { get; set; } = 0;
        public float TreshUpSignalQ { get; set; } = 0;
        public float TreshUpTx { get; set; } = 0;
        public float TreshUpRx { get; set; } = 0;
        public float TreshUpPing { get; set; } = 0;
        public float TreshUpTempOdu { get; set; } = 0;
        public float TreshUpTempIdu { get; set; } = 0;
        public float TreshUpVoltage { get; set; } = 0;

        public float TreshDownSignal { get; set; } = 0;
        public float TreshDownSignalQ { get; set; } = 0;
        public float TreshDownTx { get; set; } = 0;
        public float TreshDownRx { get; set; } = 0;
        public float TreshDownPing { get; set; } = 0;
        public float TreshDownTempOdu { get; set; } = 0;
        public float TreshDownTempIdu { get; set; } = 0;
        public float TreshDownVoltage { get; set; } = 0;
    }
}
