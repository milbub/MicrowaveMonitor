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
    public partial class EventLogPane : UserControl
    {
        private const int permittedLinesCount = 50;

        public EventLogPane()
        {
            InitializeComponent();

            log.MouseEnter += SetBoxActivity;
            log.MouseLeave += SetBoxActivity;
        }

        public void AppendNotificationDispatch(string time, string level, string text, SolidColorBrush timeColor, SolidColorBrush levelColor, SolidColorBrush textColor)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(() =>
                    {
                        AppendText(time, timeColor);
                        AppendText(level, levelColor);
                        AppendText(text, textColor);
                        Clean();
                    });
                }
                else
                {
                    AppendText(time, timeColor);
                    AppendText(level, levelColor);
                    AppendText(text, textColor);
                    Clean();
                }
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void AppendTextDispatch(string text, SolidColorBrush color)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(() =>
                    {
                        AppendText(text, color);
                    });
                }
                else
                {
                    AppendText(text, color);
                }
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void AppendText(string text, SolidColorBrush color)
        {
            TextRange tr = new TextRange(log.Document.ContentEnd, log.Document.ContentEnd);
            tr.Text = text;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);
        }

        private void Clean()
        {
            if (log.Document.Blocks.Count() > permittedLinesCount)
            {
                log.Document.Blocks.Remove(log.Document.Blocks.FirstBlock);
            }
        }

        private void SetBoxActivity(object sender, RoutedEventArgs e)
        {
            if (e.RoutedEvent.Name == MouseEnterEvent.Name)
            {
                log.Focusable = true;
            }
            else if (e.RoutedEvent.Name == MouseLeaveEvent.Name)
            {
                log.Focusable = false;
            }
        }
    }
}
