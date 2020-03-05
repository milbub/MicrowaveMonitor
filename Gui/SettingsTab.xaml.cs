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

namespace MicrowaveMonitor.Gui
{
    public partial class SettingsTab : UserControl
    {
        public SettingsTab()
        {
            InitializeComponent();
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
