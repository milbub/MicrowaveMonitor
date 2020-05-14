using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MicrowaveMonitor.Gui
{
    public struct LogRow
    {
        public string time;
        public string level;
        public string text;
        public SolidColorBrush timeColor;
        public SolidColorBrush levelColor;
        public SolidColorBrush textColor;
    }

    public partial class EventLogPane : UserControl
    {
        private const int permittedLinesCount = 150;

        public EventLogPane()
        {
            InitializeComponent();

            log.MouseEnter += SetBoxActivity;
            log.MouseLeave += SetBoxActivity;
        }

        public void AppendNotificationDispatch(LogRow row)
        {
            try
            {
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        AppendNotification(row);
                    }));
                }
                else
                {
                    AppendNotification(row);
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
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        AppendText(text, color);
                    }));
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

        private void AppendNotification(LogRow row)
        {
            AppendText(row.time, row.timeColor);
            AppendText(row.level, row.levelColor);
            AppendText(row.text, row.textColor);
            log.ScrollToEnd();
            Clean();
        }

        private void AppendText(string text, SolidColorBrush color)
        {
            TextRange tr = new TextRange(log.Document.ContentEnd, log.Document.ContentEnd) { Text = text };
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
