using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Configurations;
using MicrowaveMonitor.Database;

namespace MicrowaveMonitor.Gui
{
    public partial class GraphRealtime : UserControl, INotifyPropertyChanged
    {
        private double _axisMax;
        private double _axisMin;

        public ChartValues<Record<double>> ChartValues { get; set; }
        public Func<double, string> DateTimeFormatter { get; set; }
        public double AxisStep { get; set; }
        public double AxisUnit { get; set; }
        public bool IsReading { get; set; }

        public GraphRealtime()
        {
            InitializeComponent();

            var mapper = Mappers.Xy<Record<double>>()
                .X(model => model.TimeMark.Ticks)
                .Y(model => model.Data);

            Charting.For<Record<double>>(mapper);

            ChartValues = new ChartValues<Record<double>>();
            DateTimeFormatter = value => new DateTime((long)value).ToString("HH:mm:ss");

            AxisStep = TimeSpan.FromSeconds(5).Ticks;
            AxisUnit = TimeSpan.TicksPerSecond;

            SetAxisLimits(DateTime.Now);

            DataContext = this;
            IsReading = true;
        }

        public void Read(Record<double> record)
        {
                ChartValues.Add(record);
                SetAxisLimits(record.TimeMark);

                // only use the last 250 values
                if (ChartValues.Count > 250) ChartValues.RemoveAt(0);
        }

        private void SetAxisLimits(DateTime now)
        {
            AxisMax = now.Ticks + TimeSpan.FromSeconds(1).Ticks;    // axis ahead
            AxisMin = now.Ticks - TimeSpan.FromSeconds(59).Ticks;   // axis behind
        }

        public double AxisMax
        {
            get { return _axisMax; }
            set
            {
                _axisMax = value;
                OnPropertyChanged("AxisMax");
            }
        }
        public double AxisMin
        {
            get { return _axisMin; }
            set
            {
                _axisMin = value;
                OnPropertyChanged("AxisMin");
            }
        }

        private void InjectStopOnClick(object sender, RoutedEventArgs e)
        {
            IsReading = !IsReading;
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}