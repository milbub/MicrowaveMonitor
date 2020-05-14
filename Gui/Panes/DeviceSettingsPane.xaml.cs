using MicrowaveMonitor.Database;
using System.Globalization;
using System.Windows.Controls;

namespace MicrowaveMonitor.Gui
{
    public partial class DeviceSettingsPane : UserControl
    {
        public DeviceSettingsPane()
        {
            InitializeComponent();
        }

        public bool FillBoxes(Device device)
        {
            boxIP_A.Text = device.Address;
            boxSnmpComm_A.Text = device.CommunityString;
            switch (device.SnmpVersion)
            {
                case Device.SnmpProtocolVersion.v1:
                    cmbSnmpVer_A.SelectedIndex = 0;
                    break;
                case Device.SnmpProtocolVersion.v2:
                    cmbSnmpVer_A.SelectedIndex = 1;
                    break;
                case Device.SnmpProtocolVersion.v3:
                    cmbSnmpVer_A.SelectedIndex = 2;
                    break;
                default:
                    break;
            }
            boxSnmpPort_A.Text = device.SnmpPort.ToString(CultureInfo.InvariantCulture);
            boxLatitude.Text = device.Latitude;
            boxLongitude.Text = device.Longitude;

            monPing_A.IsChecked = device.IsWatchedPing;
            refPing_A.Text = (device.RefreshPing / 1000).ToString(CultureInfo.InvariantCulture);
            tPingCheck.IsChecked = device.TresholdPing;
            tmaxPing.Text = device.TreshUpPing.ToString(CultureInfo.InvariantCulture);
            tminPing.Text = device.TreshDownPing.ToString(CultureInfo.InvariantCulture);

            enaSignalStr_A.IsChecked = device.IsEnabledSignal;
            monSignalStr_A.IsChecked = device.IsWatchedSignal;
            refSignalStr_A.Text = (device.RefreshSignal / 1000).ToString(CultureInfo.InvariantCulture);
            boxSignalStr_A.Text = device.OidSignal_s;
            tSigCheck.IsChecked = device.TresholdSignal;
            tmaxSig.Text = device.TreshUpSignal.ToString(CultureInfo.InvariantCulture);
            tminSig.Text = device.TreshDownSignal.ToString(CultureInfo.InvariantCulture);

            enaSignalQ_A.IsChecked = device.IsEnabledSignalQ;
            monSignalQ_A.IsChecked = device.IsWatchedSignalQ;
            refSignalQ_A.Text = (device.RefreshSignalQ / 1000).ToString(CultureInfo.InvariantCulture);
            boxSignalQ_A.Text = device.OidSignalQ_s;
            divisorSignalQ.Text = device.SignalQDivisor.ToString(CultureInfo.InvariantCulture);
            tSigQCheck.IsChecked = device.TresholdSignalQ;
            tmaxSigQ.Text = device.TreshUpSignalQ.ToString(CultureInfo.InvariantCulture);
            tminSigQ.Text = device.TreshDownSignalQ.ToString(CultureInfo.InvariantCulture);

            enaTempOdu.IsChecked = device.IsEnabledTempOdu;
            monTempOdu.IsChecked = device.IsWatchedTempOdu;
            refTempOdu.Text = (device.RefreshTempOdu / 1000).ToString(CultureInfo.InvariantCulture);
            boxTempOdu.Text = device.OidTempOdu_s;
            tTempOduCheck.IsChecked = device.TresholdTempOdu;
            tmaxTempOdu.Text = device.TreshUpTempOdu.ToString(CultureInfo.InvariantCulture);
            tminTempOdu.Text = device.TreshDownTempOdu.ToString(CultureInfo.InvariantCulture);

            enaTempIdu.IsChecked = device.IsEnabledTempIdu;
            outdoorIdu.IsChecked = device.IsTempIduOutdoor;
            monTempIdu.IsChecked = device.IsWatchedTempIdu;
            refTempIdu.Text = (device.RefreshTempIdu / 1000).ToString(CultureInfo.InvariantCulture);
            boxTempIdu.Text = device.OidTempIdu_s;
            tTempIduCheck.IsChecked = device.TresholdTempIdu;
            tmaxTempIdu.Text = device.TreshUpTempIdu.ToString(CultureInfo.InvariantCulture);
            tminTempIdu.Text = device.TreshDownTempIdu.ToString(CultureInfo.InvariantCulture);

            enaVolt.IsChecked = device.IsEnabledVoltage;
            monVolt.IsChecked = device.IsWatchedVoltage;
            refVolt.Text = (device.RefreshVoltage / 1000).ToString(CultureInfo.InvariantCulture);
            boxVolt.Text = device.OidVoltage_s;
            tVoltCheck.IsChecked = device.TresholdVoltage;
            tmaxVolt.Text = device.TreshUpVoltage.ToString(CultureInfo.InvariantCulture);
            tminVolt.Text = device.TreshDownVoltage.ToString(CultureInfo.InvariantCulture);

            enaTx_A.IsChecked = device.IsEnabledTx;
            refTx_A.Text = (device.RefreshTx / 1000).ToString(CultureInfo.InvariantCulture);
            boxTx_A.Text = device.OidTxDataRate_s;

            enaRx_A.IsChecked = device.IsEnabledRx;
            refRx_A.Text = (device.RefreshRx / 1000).ToString(CultureInfo.InvariantCulture);
            boxRx_A.Text = device.OidRxDataRate_s;

            return true;
        }

        public Device SaveBoxes(Device device)
        {
            device.Address = boxIP_A.Text;
            device.CommunityString = boxSnmpComm_A.Text;
            switch (cmbSnmpVer_A.SelectedIndex)
            {
                case 0:
                    device.SnmpVersion = Device.SnmpProtocolVersion.v1;
                    break;
                case 1:
                    device.SnmpVersion = Device.SnmpProtocolVersion.v2;
                    break;
                case 2:
                    device.SnmpVersion = Device.SnmpProtocolVersion.v3;
                    break;
                default:
                    break;
            }
            device.SnmpPort = ParseInt(boxSnmpPort_A.Text);
            if (boxLatitude.Text == string.Empty)
                device.Latitude = "0";
            else
                device.Latitude = boxLatitude.Text;
            if (boxLongitude.Text == string.Empty)
                device.Longitude = "0";
            else
                device.Longitude = boxLongitude.Text;

            device.IsWatchedPing = (bool)monPing_A.IsChecked;
            device.RefreshPing = (int)(ParseFloat(refPing_A.Text) * 1000);
            device.TresholdPing = (bool)tPingCheck.IsChecked;
            device.TreshUpPing = ParseFloat(tmaxPing.Text);
            device.TreshDownPing = ParseFloat(tminPing.Text);

            device.IsEnabledSignal = (bool)enaSignalStr_A.IsChecked;
            device.IsWatchedSignal = (bool)monSignalStr_A.IsChecked;
            device.RefreshSignal = (int)(ParseFloat(refSignalStr_A.Text) * 1000);
            device.OidSignal_s = boxSignalStr_A.Text;
            device.TresholdSignal = (bool)tSigCheck.IsChecked;
            device.TreshUpSignal = ParseFloat(tmaxSig.Text);
            device.TreshDownSignal = ParseFloat(tminSig.Text);

            device.IsEnabledSignalQ = (bool)enaSignalQ_A.IsChecked;
            device.IsWatchedSignalQ = (bool)monSignalQ_A.IsChecked;
            device.RefreshSignalQ = (int)(ParseFloat(refSignalQ_A.Text) * 1000);
            device.OidSignalQ_s = boxSignalQ_A.Text;
            device.SignalQDivisor = ParseInt(divisorSignalQ.Text);
            device.TresholdSignalQ = (bool)tSigQCheck.IsChecked;
            device.TreshUpSignalQ = ParseFloat(tmaxSigQ.Text);
            device.TreshDownSignalQ = ParseFloat(tminSigQ.Text);

            device.IsEnabledTempOdu = (bool)enaTempOdu.IsChecked;
            device.IsWatchedTempOdu = (bool)monTempOdu.IsChecked;
            device.RefreshTempOdu = (int)(ParseFloat(refTempOdu.Text) * 1000);
            device.OidTempOdu_s = boxTempOdu.Text;
            device.TresholdTempOdu = (bool)tTempOduCheck.IsChecked;
            device.TreshUpTempOdu = ParseFloat(tmaxTempOdu.Text);
            device.TreshDownTempOdu = ParseFloat(tminTempOdu.Text);

            device.IsEnabledTempIdu = (bool)enaTempIdu.IsChecked;
            device.IsTempIduOutdoor = (bool)outdoorIdu.IsChecked;
            device.IsWatchedTempIdu = (bool)monTempIdu.IsChecked;
            device.RefreshTempIdu = (int)(ParseFloat(refTempIdu.Text) * 1000);
            device.OidTempIdu_s = boxTempIdu.Text;
            device.TresholdTempIdu = (bool)tTempIduCheck.IsChecked;
            device.TreshUpTempIdu = ParseFloat(tmaxTempIdu.Text);
            device.TreshDownTempIdu = ParseFloat(tminTempIdu.Text);

            device.IsEnabledVoltage = (bool)enaVolt.IsChecked;
            device.IsWatchedVoltage = (bool)monVolt.IsChecked;
            device.RefreshVoltage = (int)(ParseFloat(refVolt.Text) * 1000);
            device.OidVoltage_s = boxVolt.Text;
            device.TresholdVoltage = (bool)tVoltCheck.IsChecked;
            device.TreshUpVoltage = ParseFloat(tmaxVolt.Text);
            device.TreshDownVoltage = ParseFloat(tminVolt.Text);

            device.IsEnabledTx = (bool)enaTx_A.IsChecked;
            device.RefreshTx = (int)(ParseFloat(refTx_A.Text) * 1000);
            device.OidTxDataRate_s = boxTx_A.Text;

            device.IsEnabledRx = (bool)enaRx_A.IsChecked;
            device.RefreshRx = (int)(ParseFloat(refRx_A.Text) * 1000);
            device.OidRxDataRate_s = boxRx_A.Text;

            return device;
        }

        public bool ClearBoxes()
        {
            boxIP_A.Text = string.Empty;
            boxSnmpComm_A.Text = "public";
            cmbSnmpVer_A.SelectedIndex = 0;
            boxSnmpPort_A.Text = "161";
            boxLatitude.Text = string.Empty;
            boxLongitude.Text = string.Empty;

            monPing_A.IsChecked = true;
            refPing_A.Text = "1";
            tPingCheck.IsChecked = false;
            tmaxPing.Text = string.Empty;
            tminPing.Text = string.Empty;

            enaSignalStr_A.IsChecked = false;
            monSignalStr_A.IsChecked = false;
            refSignalStr_A.Text = string.Empty;
            boxSignalStr_A.Text = string.Empty;
            tSigCheck.IsChecked = false;
            tmaxSig.Text = string.Empty;
            tminSig.Text = string.Empty;

            enaSignalQ_A.IsChecked = false;
            monSignalQ_A.IsChecked = false;
            refSignalQ_A.Text = string.Empty;
            boxSignalQ_A.Text = string.Empty;
            divisorSignalQ.Text = string.Empty;
            tSigQCheck.IsChecked = false;
            tmaxSigQ.Text = string.Empty;
            tminSigQ.Text = string.Empty;

            enaTempOdu.IsChecked = false;
            monTempOdu.IsChecked = false;
            refTempOdu.Text = string.Empty;
            boxTempOdu.Text = string.Empty;
            tTempOduCheck.IsChecked = false;
            tmaxTempOdu.Text = string.Empty;
            tminTempOdu.Text = string.Empty;

            enaTempIdu.IsChecked = false;
            outdoorIdu.IsChecked = false;
            monTempIdu.IsChecked = false;
            refTempIdu.Text = string.Empty;
            boxTempIdu.Text = string.Empty;
            tTempIduCheck.IsChecked = false;
            tmaxTempIdu.Text = string.Empty;
            tminTempIdu.Text = string.Empty;

            enaVolt.IsChecked = false;
            monVolt.IsChecked = false;
            refVolt.Text = string.Empty;
            boxVolt.Text = string.Empty;
            tVoltCheck.IsChecked = false;
            tmaxVolt.Text = string.Empty;
            tminVolt.Text = string.Empty;

            enaTx_A.IsChecked = false;
            refTx_A.Text = string.Empty;
            boxTx_A.Text = string.Empty;

            enaRx_A.IsChecked = false;
            refRx_A.Text = string.Empty;
            boxRx_A.Text = string.Empty;

            return false;
        }

        static float ParseFloat(string s)
        {
            if (float.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out float res))
                return res;
            else
                return 0;
        }

        static int ParseInt(string s)
        {
            if (float.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out float res))
                return (int)res;
            else
                return 0;
        }
    }
}
