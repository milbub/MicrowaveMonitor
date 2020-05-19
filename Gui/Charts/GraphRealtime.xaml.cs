using LiveCharts;
using LiveCharts.Configurations;
using MicrowaveMonitor.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MicrowaveMonitor.Gui
{
    public partial class GraphRealtime : UserControl, INotifyPropertyChanged
    {
        private double _axisMax;
        private double _axisMin;
        private double _axisStep;
        private double _axisUnit;
        private Func<double, string> _dateTimeFormatter;
        private bool _disableAnimations;

        public ChartValues<Record<double>> ChartValues { get; set; }
        public bool IsReading { get; set; }
        public int Span { get; private set; }
        public int DevId { get; private set; }

        public GraphRealtime()
        {
            InitializeComponent();

            var mapper = Mappers.Xy<Record<double>>()
                .X(model => model.TimeMark.Ticks)
                .Y(model => model.Data);

            Charting.For<Record<double>>(mapper);

            ChartValues = new ChartValues<Record<double>>();
            DateTimeFormatter = value => new DateTime((long)value).ToString("HH:mm:ss");

            SetAxisGrid(60, 1);
            SetAxisLimits(DateTime.Now, 600);
            DisableAnimations = true;

            DataContext = this;
            IsReading = true;
        }

        public void Read(Record<double> record, int resolution, int span)
        {
            ChartValues.Add(record);
            SetAxisLimits(record.TimeMark, span);

            if (ChartValues.Count > (resolution + 2))
                ChartValues.RemoveAt(0);
        }

        public void ReadMany(List<Record<double>> records, int resolution, int span, int device)
        {
            if (records.Count > 0)
            {
                ChartValues.AddRange(records);
                SetAxisLimits(records.Last().TimeMark, span);
            }

            resolution += 2;
            if (ChartValues.Count > resolution)
                for (int i = 0; i < (ChartValues.Count - resolution); i++)
                {
                    ChartValues.RemoveAt(0);
                }

            Span = span;
            DevId = device;
        }

        public void SetAxisLimits(DateTime timestamp, int span)
        {
            AxisMax = timestamp.Ticks + TimeSpan.FromSeconds(1).Ticks;          // axis ahead
            AxisMin = timestamp.Ticks - TimeSpan.FromSeconds(span - 1).Ticks;   // axis behind

            OnPropertyChanged("AxisUnit");
            OnPropertyChanged("AxisStep");
        }

        public void SetAxisGrid(int step, int unit)
        {
            AxisStep = TimeSpan.FromSeconds(step).Ticks;
            AxisUnit = TimeSpan.FromSeconds(unit).Ticks;

            switch (step)
            {
                case int n when (n < 300):
                    DateTimeFormatter = value => new DateTime((long)value).ToString("HH:mm:ss");
                    break;
                case int n when (n <= 7200 && n >= 300):
                    DateTimeFormatter = value => new DateTime((long)value).ToString("HH:mm");
                    break;
                case int n when (n <= 50400 && n > 7200):
                    DateTimeFormatter = value => new DateTime((long)value).ToString("dd.MM HH:mm");
                    break;
                case int n when (n >= 50400):
                    DateTimeFormatter = value => new DateTime((long)value).ToString("dd.MM");
                    break;
                default:
                    break;
            }
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

        public double AxisStep
        {
            get { return _axisStep; }
            set { _axisStep = value; }      // notification is done after axis limits change
        }
        public double AxisUnit
        {
            get { return _axisUnit; }
            set { _axisUnit = value; }      // notification is done after axis limits change
        }

        public Func<double, string> DateTimeFormatter
        { 
            get { return _dateTimeFormatter; }
            set
            {
                _dateTimeFormatter = value;
                OnPropertyChanged("DateTimeFormatter");
            }
        }

        public bool DisableAnimations
        {
            get { return _disableAnimations; }
            set
            {
                _disableAnimations = value;
                OnPropertyChanged("DisableAnimations");
            }
        }

        private void InjectStopOnClick(object sender, RoutedEventArgs e)
        {
            IsReading = !IsReading;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}