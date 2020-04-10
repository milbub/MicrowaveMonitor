using System;
using System.Windows;

namespace MicrowaveMonitor.Gui
{
    public partial class NewLinkWindow : Window
    {
        public string NameAnswer
        {
            get { return nameBox.Text; }
        }

        public NewLinkWindow()
        {
            InitializeComponent();
        }

        private void OkFired(object sender, RoutedEventArgs e)
        {
            if (nameBox.Text == "")
                blank.Visibility = Visibility.Visible;
            else
                DialogResult = true;
        }

        private void SetFocus(object sender, EventArgs e)
        {
            nameBox.Focus();
        }
    }
}
