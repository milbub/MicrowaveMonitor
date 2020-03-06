using System;
using System.ComponentModel;

namespace MicrowaveMonitor.Database
{
    public class DeviceDisplay : INotifyPropertyChanged
    {
        private double _avgSig = 0;
        private double _diffSig = 0;
        private double _avgSigQ = 0;
        private double _diffSigQ = 0;
        private double _avgPing = 0;
        private double _diffPing = 0;
        private Record<double> _dataSig;
        private Record<double> _dataSigQ;
        private Record<uint> _dataTx;
        private Record<uint> _dataRx;
        private Record<double> _dataPing;
        private string _sysName = String.Empty;
        private uint _uptime = 0;
        private string _weatherIcon = "03d";
        private string _weatherDesc = "no data";
        private int _weatherTemp = 0;
        private double _weatherWind = 0;

        public double AvgSig
        {
            get => _avgSig;
            set => _avgSig = value;
        }
        public double DiffSig
        {
            get => _diffSig;
            set
            {
                if (value != _diffSig)
                {
                    _diffSig = value;
                    OnPropertyChanged(nameof(DiffSig));
                }
            }
        }
        public double AvgSigQ
        {
            get => _avgSigQ;
            set => _avgSigQ = value;
        }
        public double DiffSigQ
        {
            get => _diffSigQ;
            set
            {
                if (value != _diffSigQ)
                {
                    _diffSigQ = value;
                    OnPropertyChanged(nameof(DiffSigQ));
                }
            }
        }
        public double AvgPing
        {
            get => _avgPing;
            set => _avgPing = value;
        }
        public double DiffPing
        {
            get => _diffPing;
            set
            {
                if (value != _diffPing)
                {
                    _diffPing = value;
                    OnPropertyChanged(nameof(DiffPing));
                }
            }
        }
        public Record<double> DataSig
        {
            get => _dataSig;
            set
            {
                if (value != _dataSig)
                {
                    _dataSig = value;
                    OnPropertyChanged(nameof(DataSig));
                }
            }
        }
        public Record<double> DataSigQ
        {
            get => _dataSigQ;
            set
            {
                if (value != _dataSigQ)
                {
                    _dataSigQ = value;
                    OnPropertyChanged(nameof(DataSigQ));
                }
            }
        }
        public Record<uint> DataTx
        {
            get => _dataTx;
            set
            {
                if (value != _dataTx)
                {
                    _dataTx = value;
                    OnPropertyChanged(nameof(DataTx));
                }
            }
        }
        public Record<uint> DataRx
        {
            get => _dataRx;
            set
            {
                if (value != _dataRx)
                {
                    _dataRx = value;
                    OnPropertyChanged(nameof(DataRx));
                }
            }
        }
        public Record<double> DataPing
        {
            get => _dataPing;
            set
            {
                if (value != _dataPing)
                {
                    _dataPing = value;
                    OnPropertyChanged(nameof(DataPing));
                }
            }
        }

        public string SysName
        {
            get => _sysName;
            set
            {
                if (value != _sysName)
                {
                    _sysName = value;
                    OnPropertyChanged(nameof(SysName));
                }
            }
        }
        public uint Uptime
        {
            get => _uptime;
            set
            {
                if (value != _uptime)
                {
                    _uptime = value;
                    OnPropertyChanged(nameof(Uptime));
                }
            }
        }

        public string WeatherIcon
        {
            get => _weatherIcon;
            set
            {
                if (value != _weatherIcon)
                {
                    _weatherIcon = value;
                    OnPropertyChanged(nameof(WeatherIcon));
                }
            }
        }
        public string WeatherDesc
        {
            get => _weatherDesc;
            set
            {
                if (value != _weatherDesc)
                {
                    _weatherDesc = value;
                    OnPropertyChanged(nameof(WeatherDesc));
                }
            }
        }
        public int WeatherTemp
        {
            get => _weatherTemp;
            set
            {
                if (value != _weatherTemp)
                {
                    _weatherTemp = value;
                    OnPropertyChanged(nameof(WeatherTemp));
                }
            }
        }
        public double WeatherWind
        {
            get => _weatherWind;
            set
            {
                if (value != _weatherWind)
                {
                    _weatherWind = value;
                    OnPropertyChanged(nameof(WeatherWind));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
