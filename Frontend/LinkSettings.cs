using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using MicrowaveMonitor.Gui;

namespace MicrowaveMonitor.Frontend
{
    internal class LinkSettings : Renderer
    {
        private LinkView _view;

        public LinkView View { get => _view; }

        public LinkSettings(MonitoringWindow monitorGui, LinkView view) : base(monitorGui)
        {
            _view = view;
            ChangeSettings();
        }

        public void ChangeSettings()
        {
            try
            {
                MonitorGui.UpdateElementText(_monitorGui.boxLinkName, View.ViewedLink.Name);
                MonitorGui.UpdateElementText(_monitorGui.boxNote, View.ViewedLink.Note);

                MonitorGui.UpdateElementText(_monitorGui.boxIP_A, View.ViewedLink.BaseDevice.Address.Address.ToString());
                MonitorGui.UpdateElementText(_monitorGui.boxSignalStr_A, View.ViewedLink.BaseDevice.OidSignal.ToString());
                MonitorGui.UpdateElementText(_monitorGui.boxSignalQ_A, View.ViewedLink.BaseDevice.OidSignalQ.ToString());
                MonitorGui.UpdateElementText(_monitorGui.boxTx_A, View.ViewedLink.BaseDevice.OidTxDataRate.ToString());
                MonitorGui.UpdateElementText(_monitorGui.boxRx_A, View.ViewedLink.BaseDevice.OidRxDataRate.ToString());
                MonitorGui.UpdateElementText(_monitorGui.refSignalStr_A, View.ViewedLink.BaseDevice.RefreshSignal.ToString());
                MonitorGui.UpdateElementText(_monitorGui.refSignalQ_A, View.ViewedLink.BaseDevice.RefreshSignalQ.ToString());
                MonitorGui.UpdateElementText(_monitorGui.refTx_A, View.ViewedLink.BaseDevice.RefreshTx.ToString());
                MonitorGui.UpdateElementText(_monitorGui.refRx_A, View.ViewedLink.BaseDevice.RefreshRx.ToString());
                MonitorGui.UpdateElementText(_monitorGui.refPing_A, View.ViewedLink.BaseDevice.RefreshPing.ToString());

                MonitorGui.UpdateElementText(_monitorGui.boxIP_B, View.ViewedLink.EndDevice.Address.Address.ToString());
                MonitorGui.UpdateElementText(_monitorGui.boxSignalStr_B, View.ViewedLink.EndDevice.OidSignal.ToString());
                MonitorGui.UpdateElementText(_monitorGui.boxSignalQ_B, View.ViewedLink.EndDevice.OidSignalQ.ToString());
                MonitorGui.UpdateElementText(_monitorGui.boxTx_B, View.ViewedLink.EndDevice.OidTxDataRate.ToString());
                MonitorGui.UpdateElementText(_monitorGui.boxRx_B, View.ViewedLink.EndDevice.OidRxDataRate.ToString());
                MonitorGui.UpdateElementText(_monitorGui.refSignalStr_B, View.ViewedLink.EndDevice.RefreshSignal.ToString());
                MonitorGui.UpdateElementText(_monitorGui.refSignalQ_B, View.ViewedLink.EndDevice.RefreshSignalQ.ToString());
                MonitorGui.UpdateElementText(_monitorGui.refTx_B, View.ViewedLink.EndDevice.RefreshTx.ToString());
                MonitorGui.UpdateElementText(_monitorGui.refRx_B, View.ViewedLink.EndDevice.RefreshRx.ToString());
                MonitorGui.UpdateElementText(_monitorGui.refPing_B, View.ViewedLink.EndDevice.RefreshPing.ToString());
            }
            catch(NullReferenceException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
