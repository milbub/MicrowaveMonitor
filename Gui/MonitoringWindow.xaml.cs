﻿using MicrowaveMonitor.Frontend;
using MicrowaveMonitor.Managers;
using MicrowaveMonitor.Database;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MicrowaveMonitor.Gui
{
    public partial class MonitoringWindow : Window
    {
        public enum DeviceType { Base, End, R1, R2, R3, R4 };
        private LinkManager linkManager;
        private WorkerManager workerManager;
        private AlarmManager alarmManager;
        
        private LinkView view;
        private LinkSettings settings;

        public MonitoringWindow(LinkManager linkManager, WorkerManager workerManager, AlarmManager alarmManager)
        {
            this.linkManager = linkManager;
            this.workerManager = workerManager;
            this.alarmManager = alarmManager;

            InitializeComponent();

            LinksList.ItemsSource = linkManager.LinkNames.Values;
            LinksList.SelectedItem = linkManager.LinkNames.First().Value;
            view = new LinkView(this, linkManager.LinkDatabase.Get<Link>(linkManager.LinkNames.First().Key), workerManager.DeviceToFront);
            settings = new LinkSettings(this, view);
            
            AlarmsList.ItemsSource = alarmManager.Alarms;

            LinksList.SelectionChanged += LinkChoosed;

            siteA.Checked += SiteChoosed;
            siteB.Checked += SiteChoosed;
            siteR1.Checked += SiteChoosed;
            siteR2.Checked += SiteChoosed;
            siteR3.Checked += SiteChoosed;
            siteR4.Checked += SiteChoosed;

            graphsA.MouseEnter += SetBoxActivity;
            graphsA.MouseLeave += SetBoxActivity;
            graphsB.MouseEnter += SetBoxActivity;
            graphsB.MouseLeave += SetBoxActivity;
        }

        public Device GetDevice(int id)
        {
            return linkManager.GetDevice(id);
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
            view.ChangeLink(linkManager.LinkDatabase.Get<Link>(linkManager.LinkNames.FirstOrDefault(x => x.Value == (string)LinksList.SelectedItem).Key));
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
