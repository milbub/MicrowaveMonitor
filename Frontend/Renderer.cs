using MicrowaveMonitor.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicrowaveMonitor.Frontend
{
    internal class Renderer
    {
        protected MonitoringWindow _monitorGui;

        public MonitoringWindow MonitorGui { get => _monitorGui; }

        public Renderer(MonitoringWindow monitorGui)
        {
            _monitorGui = monitorGui;
        }
    }
}
