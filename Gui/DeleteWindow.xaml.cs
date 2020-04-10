using System;
using System.Windows;


namespace MicrowaveMonitor.Gui
{
    public partial class DeleteWindow : Window
    {
        public DeleteWindow()
        {
            InitializeComponent();
        }

        private void SetFocus(object sender, EventArgs e)
        {
            confirmBox.SelectAll();
            confirmBox.Focus();
        }

        private void OkFired(object sender, RoutedEventArgs e)
        {
            if (confirmBox.Text == "DELETE" || confirmBox.Text == "delete")
                DialogResult = true;
            else
                notify.Visibility = Visibility.Visible;
        }
    }
}
