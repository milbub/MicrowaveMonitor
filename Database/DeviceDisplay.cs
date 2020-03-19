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
        private Record<double> _dataTempOdu;
        private Record<double> _dataTempIdu;
        private Record<double> _dataVoltage;
        private string _sysName = String.Empty;
        private uint _uptime = 0;
        private string _weatherIcon = "03d";
        private string _weatherDesc = "no data";
        private float _weatherTemp = 0;
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
                    OnPropertyChanged("DiffSig");
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
                    OnPropertyChanged("DiffSigQ");
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
                    OnPropertyChanged("DiffPing");
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
                    OnPropertyChanged("DataSig");
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
                    OnPropertyChanged("DataSigQ");
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
                    OnPropertyChanged("DataTx");
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
                    OnPropertyChanged("DataRx");
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
                    OnPropertyChanged("DataPing");
                }
            }
        }

        public Record<double> DataTempOdu
        {
            get => _dataTempOdu;
            set
            {
                if (value != _dataTempOdu)
                {
                    _dataTempOdu = value;
                    OnPropertyChanged("DataTempOdu");
                }
            }
        }

        public Record<double> DataTempIdu
        {
            get => _dataTempIdu;
            set
            {
                if (value != _dataTempIdu)
                {
                    _dataTempIdu = value;
                    OnPropertyChanged("DataTempIdu");
                }
            }
        }

        public Record<double> DataVoltage
        {
            get => _dataVoltage;
            set
            {
                if (value != _dataVoltage)
                {
                    _dataVoltage = value;
                    OnPropertyChanged("DataVoltage");
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
                    OnPropertyChanged("SysName");
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
                    OnPropertyChanged("Uptime");
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
                    OnPropertyChanged("WeatherIcon");
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
                    OnPropertyChanged("WeatherDesc");
                }
            }
        }
        public float WeatherTemp
        {
            get => _weatherTemp;
            set
            {
                if (value != _weatherTemp)
                {
                    _weatherTemp = value;
                    OnPropertyChanged("WeatherTemp");
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
                    OnPropertyChanged("WeatherWind");
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
