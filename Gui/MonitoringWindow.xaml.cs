using MicrowaveMonitor.Frontend;
using MicrowaveMonitor.Managers;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MicrowaveMonitor.Gui
{
    public partial class MonitoringWindow : Window
    {
        private LinkManager linkManager;
        private AlarmManager alarmManager;
        
        private LinkView view;
        private LinkSettings settings;

        public MonitoringWindow(LinkManager linkManager, AlarmManager alarmManager)
        {
            this.linkManager = linkManager;
            this.alarmManager = alarmManager;

            InitializeComponent();

            siteA.Checked += SiteChoosed;
            siteB.Checked += SiteChoosed;
            siteR1.Checked += SiteChoosed;
            siteR2.Checked += SiteChoosed;
            siteR3.Checked += SiteChoosed;
            siteR4.Checked += SiteChoosed;

            LinksList.ItemsSource = linkManager.LinkDatabase.Keys;
            LinksList.SelectedItem = linkManager.LinkDatabase.First().Key;
            LinksList.SelectionChanged += LinkChoosed;
            
            AlarmsList.ItemsSource = alarmManager.Alarms;

            view = new LinkView(this, linkManager.LinkDatabase.First().Value);
            settings = new LinkSettings(this, view);

            graphsA.MouseEnter += SetBoxActivity;
            graphsA.MouseLeave += SetBoxActivity;
            graphsB.MouseEnter += SetBoxActivity;
            graphsB.MouseLeave += SetBoxActivity;
        }

        public void UpdateElementContent(ContentControl element, string value)
        {
            try
            {
                if (!element.Dispatcher.CheckAccess())
                {
                    element.Dispatcher.Invoke(() =>
                    {
                        element.Content = value;
                    });
                }
                else
                {
                    element.Content = value;
                }
            }
            catch (TaskCanceledException e)
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

        private void SiteChoosed(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb.IsChecked.Value)
                view.ChangeDevice(rb.Content.ToString());
        }

        private void LinkChoosed(object sender, SelectionChangedEventArgs e)
        {
            view.ChangeLink(linkManager.LinkDatabase[(string)LinksList.SelectedItem]);
            settings.ChangeSettings();
        }

        public void SiteChooserEnabler(bool state, RadioButton rb)
        {
            rb.IsEnabled = state;
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
                signalLevel.Focusable = true;
                signalQuality.Focusable = true;
                pingwin.Focusable = true;
                tx.Focusable = true;
                rx.Focusable = true;
            }
            else if (e.RoutedEvent.Name == MouseLeaveEvent.Name)
            {
                signalLevel.Focusable = false;
                signalQuality.Focusable = false;
                pingwin.Focusable = false;
                tx.Focusable = false;
                rx.Focusable = false;
            }
        }

        public void ResetView()
        {
            ip.Content = String.Empty;
            unitname.Content = String.Empty;
            ping.Content = String.Empty;
            uptime.Content = String.Empty;
            signalLevel.Text = String.Empty;
            signalQuality.Text = String.Empty;
            tx.Text = String.Empty;
            rx.Text = String.Empty;
            pingwin.Text = String.Empty;
        }
    }
}
