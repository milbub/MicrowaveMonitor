using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MicrowaveMonitor.Frontend;
using MicrowaveMonitor.Managers;

namespace MicrowaveMonitor.Gui
{
    public partial class SettingsTab : UserControl
    {
        public SettingsTab()
        {
            InitializeComponent();
        }

        internal void FillBoxes(LinkView view, LinkManager manager)
        {
            try
            {
                UpdateElementText(boxLinkName, view.ViewedLink.Name);
                UpdateElementText(boxNote, view.ViewedLink.Note);

                UpdateElementText(boxIP_A, manager.GetDevice(view.ViewedLink.DeviceBaseId).Address);
                UpdateElementText(boxSnmpComm_A, manager.GetDevice(view.ViewedLink.DeviceBaseId).CommunityString);
                UpdateElementText(boxSnmpPort_A, manager.GetDevice(view.ViewedLink.DeviceBaseId).SnmpPort.ToString());
                UpdateElementText(boxSignalStr_A, manager.GetDevice(view.ViewedLink.DeviceBaseId).OidSignal_s);
                UpdateElementText(boxSignalQ_A, manager.GetDevice(view.ViewedLink.DeviceBaseId).OidSignalQ_s);
                UpdateElementText(boxTx_A, manager.GetDevice(view.ViewedLink.DeviceBaseId).OidTxDataRate_s);
                UpdateElementText(boxRx_A, manager.GetDevice(view.ViewedLink.DeviceBaseId).OidRxDataRate_s);
                UpdateElementText(refSignalStr_A, manager.GetDevice(view.ViewedLink.DeviceBaseId).RefreshSignal.ToString());
                UpdateElementText(refSignalQ_A, manager.GetDevice(view.ViewedLink.DeviceBaseId).RefreshSignalQ.ToString());
                UpdateElementText(refTx_A, manager.GetDevice(view.ViewedLink.DeviceBaseId).RefreshTx.ToString());
                UpdateElementText(refRx_A, manager.GetDevice(view.ViewedLink.DeviceBaseId).RefreshRx.ToString());
                UpdateElementText(refPing_A, manager.GetDevice(view.ViewedLink.DeviceBaseId).RefreshPing.ToString());

                UpdateElementText(boxIP_B, manager.GetDevice(view.ViewedLink.DeviceEndId).Address);
                UpdateElementText(boxSNMP_B, manager.GetDevice(view.ViewedLink.DeviceBaseId).CommunityString);
                UpdateElementText(boxSignalStr_B, manager.GetDevice(view.ViewedLink.DeviceEndId).OidSignal_s);
                UpdateElementText(boxSignalQ_B, manager.GetDevice(view.ViewedLink.DeviceEndId).OidSignalQ_s);
                UpdateElementText(boxTx_B, manager.GetDevice(view.ViewedLink.DeviceEndId).OidTxDataRate_s);
                UpdateElementText(boxRx_B, manager.GetDevice(view.ViewedLink.DeviceEndId).OidRxDataRate_s);
                UpdateElementText(refSignalStr_B, manager.GetDevice(view.ViewedLink.DeviceEndId).RefreshSignal.ToString());
                UpdateElementText(refSignalQ_B, manager.GetDevice(view.ViewedLink.DeviceEndId).RefreshSignalQ.ToString());
                UpdateElementText(refTx_B, manager.GetDevice(view.ViewedLink.DeviceEndId).RefreshTx.ToString());
                UpdateElementText(refRx_B, manager.GetDevice(view.ViewedLink.DeviceEndId).RefreshRx.ToString());
                UpdateElementText(refPing_B, manager.GetDevice(view.ViewedLink.DeviceEndId).RefreshPing.ToString());
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
