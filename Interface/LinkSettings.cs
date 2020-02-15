using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MicrowaveMonitor.Interface
{
    internal class LinkSettings
    {
        private MonitoringWindow _monitorGui;
        private LinkView _view;

        public MonitoringWindow MonitorGui { get => _monitorGui; }
        public LinkView View { get => _view; }

        public LinkSettings(MonitoringWindow monitorGui, LinkView view)
        {
            _monitorGui = monitorGui;
            _view = view;
            ChangeSettings();
        }

        public void ChangeSettings()
        {
            try
            {
                updateBox(_monitorGui.boxLinkName, View.ViewedLink.Name);
                updateBox(_monitorGui.boxNote, View.ViewedLink.Note);

                updateBox(_monitorGui.boxIP_A, View.ViewedLink.BaseDevice.Address.Address.ToString());
                updateBox(_monitorGui.boxSignalStr_A, View.ViewedLink.BaseDevice.OidSignal.ToString());
                updateBox(_monitorGui.boxSignalQ_A, View.ViewedLink.BaseDevice.OidSignalQ.ToString());
                updateBox(_monitorGui.boxTx_A, View.ViewedLink.BaseDevice.OidTxDataRate.ToString());
                updateBox(_monitorGui.boxRx_A, View.ViewedLink.BaseDevice.OidRxDataRate.ToString());
                updateBox(_monitorGui.refSignalStr_A, View.ViewedLink.BaseDevice.RefreshSignal.ToString());
                updateBox(_monitorGui.refSignalQ_A, View.ViewedLink.BaseDevice.RefreshSignalQ.ToString());
                updateBox(_monitorGui.refTx_A, View.ViewedLink.BaseDevice.RefreshTx.ToString());
                updateBox(_monitorGui.refRx_A, View.ViewedLink.BaseDevice.RefreshRx.ToString());
                updateBox(_monitorGui.refPing_A, View.ViewedLink.BaseDevice.RefreshPing.ToString());

                updateBox(_monitorGui.boxIP_B, View.ViewedLink.EndDevice.Address.Address.ToString());
                updateBox(_monitorGui.boxSignalStr_B, View.ViewedLink.EndDevice.OidSignal.ToString());
                updateBox(_monitorGui.boxSignalQ_B, View.ViewedLink.EndDevice.OidSignalQ.ToString());
                updateBox(_monitorGui.boxTx_B, View.ViewedLink.EndDevice.OidTxDataRate.ToString());
                updateBox(_monitorGui.boxRx_B, View.ViewedLink.EndDevice.OidRxDataRate.ToString());
                updateBox(_monitorGui.refSignalStr_B, View.ViewedLink.EndDevice.RefreshSignal.ToString());
                updateBox(_monitorGui.refSignalQ_B, View.ViewedLink.EndDevice.RefreshSignalQ.ToString());
                updateBox(_monitorGui.refTx_B, View.ViewedLink.EndDevice.RefreshTx.ToString());
                updateBox(_monitorGui.refRx_B, View.ViewedLink.EndDevice.RefreshRx.ToString());
                updateBox(_monitorGui.refPing_B, View.ViewedLink.EndDevice.RefreshPing.ToString());
            }
            catch(NullReferenceException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void updateBox(TextBox element, string value)
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
