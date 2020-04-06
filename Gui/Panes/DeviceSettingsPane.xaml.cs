using System;
using System.Threading.Tasks;
using System.Windows.Controls;

using MicrowaveMonitor.Managers;

namespace MicrowaveMonitor.Gui
{
    public partial class DeviceSettingsPane : UserControl
    {
        public DeviceSettingsPane()
        {
            InitializeComponent();
        }

        public bool FillBoxes(LinkManager manager, int deviceId)
        {
            boxIP_A.Text = manager.GetDevice(deviceId).Address;
            boxSnmpComm_A.Text = manager.GetDevice(deviceId).CommunityString;
            boxSnmpPort_A.Text = manager.GetDevice(deviceId).SnmpPort.ToString();
            boxSignalStr_A.Text = manager.GetDevice(deviceId).OidSignal_s;
            boxSignalQ_A.Text = manager.GetDevice(deviceId).OidSignalQ_s;
            boxTx_A.Text = manager.GetDevice(deviceId).OidTxDataRate_s;
            boxRx_A.Text = manager.GetDevice(deviceId).OidRxDataRate_s;
            refSignalStr_A.Text = manager.GetDevice(deviceId).RefreshSignal.ToString();
            refSignalQ_A.Text = manager.GetDevice(deviceId).RefreshSignalQ.ToString();
            refTx_A.Text = manager.GetDevice(deviceId).RefreshTx.ToString();
            refRx_A.Text = manager.GetDevice(deviceId).RefreshRx.ToString();
            refPing_A.Text = manager.GetDevice(deviceId).RefreshPing.ToString();
            
            return true;
        }
    }
}
