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
        public EventLogPane()
        {
            InitializeComponent();

            eventLog.MouseEnter += SetBoxActivity;
            eventLog.MouseLeave += SetBoxActivity;
        }

        public string LogWindowUpdate(TextBox logWindow, string newMessage, string tempMessage)
        {
            try
            {
                if (!logWindow.Dispatcher.CheckAccess())
                {
                    logWindow.Dispatcher.Invoke(() =>
                    {
                        if (logWindow.IsSelectionActive)
                        {
                            tempMessage += newMessage;
                        }
                        else
                        {
                            logWindow.Text += tempMessage + newMessage;
                            tempMessage = String.Empty;
                            logWindow.ScrollToEnd();
                        }
                    });
                }
                else
                {
                    if (logWindow.IsSelectionActive)
                    {
                        tempMessage += newMessage;
                    }
                    else
                    {
                        logWindow.Text += tempMessage + newMessage;
                        tempMessage = String.Empty;
                        logWindow.ScrollToEnd();
                    }
                }
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine(e.Message);
            }

            LogWindowCleaner(logWindow, 50);
            return tempMessage;
        }

        private void LogWindowCleaner(TextBox logWindow, int permittedLinesCount)
        {
            try
            {
                if (!logWindow.Dispatcher.CheckAccess())
                {
                    logWindow.Dispatcher.Invoke(() =>
                    {
                        var splitted = logWindow.Text.Split('\n');
                        int linesCount = splitted.Length;
                        if (linesCount > permittedLinesCount)
                        {
                            logWindow.Text = String.Join("\n", splitted.Skip(linesCount - permittedLinesCount));
                        }
                    });
                }
                else
                {
                    var splitted = logWindow.Text.Split('\n');
                    int linesCount = splitted.Length;
                    if (linesCount > permittedLinesCount)
                    {
                        logWindow.Text = String.Join("\n", splitted.Skip(linesCount - permittedLinesCount));
                    }
                }
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void SetBoxActivity(object sender, RoutedEventArgs e)
        {
            if (e.RoutedEvent.Name == MouseEnterEvent.Name)
            {
                eventLog.Focusable = true;
            }
            else if (e.RoutedEvent.Name == MouseLeaveEvent.Name)
            {
                eventLog.Focusable = false;
            }
        }
    }
}
