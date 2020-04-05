using System;
using System.Threading.Tasks;
using System.Windows.Controls;

using MicrowaveMonitor.Managers;

namespace MicrowaveMonitor.Gui
{
    public partial class SettingsTab : UserControl
    {
        public SettingsTab()
        {
            InitializeComponent();
        }

        internal void FillBoxes(MonitoringWindow window, LinkManager manager)
        {
            try
            {
                UpdateElementText(boxLinkName, window.viewedLink.Name);
                UpdateElementText(boxNote, window.viewedLink.Note);

                UpdateElementText(boxIP_A, manager.GetDevice(window.viewedLink.DeviceBaseId).Address);
                UpdateElementText(boxSnmpComm_A, manager.GetDevice(window.viewedLink.DeviceBaseId).CommunityString);
                UpdateElementText(boxSnmpPort_A, manager.GetDevice(window.viewedLink.DeviceBaseId).SnmpPort.ToString());
                UpdateElementText(boxSignalStr_A, manager.GetDevice(window.viewedLink.DeviceBaseId).OidSignal_s);
                UpdateElementText(boxSignalQ_A, manager.GetDevice(window.viewedLink.DeviceBaseId).OidSignalQ_s);
                UpdateElementText(boxTx_A, manager.GetDevice(window.viewedLink.DeviceBaseId).OidTxDataRate_s);
                UpdateElementText(boxRx_A, manager.GetDevice(window.viewedLink.DeviceBaseId).OidRxDataRate_s);
                UpdateElementText(refSignalStr_A, manager.GetDevice(window.viewedLink.DeviceBaseId).RefreshSignal.ToString());
                UpdateElementText(refSignalQ_A, manager.GetDevice(window.viewedLink.DeviceBaseId).RefreshSignalQ.ToString());
                UpdateElementText(refTx_A, manager.GetDevice(window.viewedLink.DeviceBaseId).RefreshTx.ToString());
                UpdateElementText(refRx_A, manager.GetDevice(window.viewedLink.DeviceBaseId).RefreshRx.ToString());
                UpdateElementText(refPing_A, manager.GetDevice(window.viewedLink.DeviceBaseId).RefreshPing.ToString());

                UpdateElementText(boxIP_B, manager.GetDevice(window.viewedLink.DeviceEndId).Address);
                UpdateElementText(boxSNMP_B, manager.GetDevice(window.viewedLink.DeviceBaseId).CommunityString);
                UpdateElementText(boxSignalStr_B, manager.GetDevice(window.viewedLink.DeviceEndId).OidSignal_s);
                UpdateElementText(boxSignalQ_B, manager.GetDevice(window.viewedLink.DeviceEndId).OidSignalQ_s);
                UpdateElementText(boxTx_B, manager.GetDevice(window.viewedLink.DeviceEndId).OidTxDataRate_s);
                UpdateElementText(boxRx_B, manager.GetDevice(window.viewedLink.DeviceEndId).OidRxDataRate_s);
                UpdateElementText(refSignalStr_B, manager.GetDevice(window.viewedLink.DeviceEndId).RefreshSignal.ToString());
                UpdateElementText(refSignalQ_B, manager.GetDevice(window.viewedLink.DeviceEndId).RefreshSignalQ.ToString());
                UpdateElementText(refTx_B, manager.GetDevice(window.viewedLink.DeviceEndId).RefreshTx.ToString());
                UpdateElementText(refRx_B, manager.GetDevice(window.viewedLink.DeviceEndId).RefreshRx.ToString());
                UpdateElementText(refPing_B, manager.GetDevice(window.viewedLink.DeviceEndId).RefreshPing.ToString());
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void UpdateElementText(TextBox element, string value)
        {
            try
            {
                if (!element.Dispatcher.CheckAccess())
                {
                    element.Dispatcher.Invoke(() =>
                    {
                        element.Text = value;
                    });
                }
                else
                {
                    element.Text = value;
                }
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
