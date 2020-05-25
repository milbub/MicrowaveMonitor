using MicrowaveMonitor.Managers;
using MicrowaveMonitor.Properties;
using System;
using System.Globalization;
using System.Windows;

namespace MicrowaveMonitor.Gui
{
    public partial class SettingsWindow : Window
    {
        private readonly AlarmManager alarmMan;

        public SettingsWindow(AlarmManager manager)
        {
            InitializeComponent();
            alarmMan = manager;

            FillBoxes();
        }

        private void FillBoxes()
        {
            avgXEnabled.IsChecked = Settings.Default.a_enable_longavg;
            avgXpercSignal.Value = Settings.Default.a_longavg_percDiff_sig;
            avgXpercSignalQ.Value = Settings.Default.a_longavg_percDiff_sigQ;
            avgXpercTempIdu.Value = Settings.Default.a_longavg_percDiff_TmpI;
            avgXpercVolt.Value = Settings.Default.a_longavg_percDiff_volt;
            avgXpercPing.Value = Settings.Default.a_longavg_percDiff_late;

            avgYEnabled.IsChecked = Settings.Default.a_enable_shortavg;
            avgYpercSignal.Value = Settings.Default.a_shortavg_percDiff_sig;
            avgYpercSignalQ.Value = Settings.Default.a_shortavg_percDiff_sigQ;
            avgYpercTempIdu.Value = Settings.Default.a_shortavg_percDiff_TmpI;
            avgYpercVolt.Value = Settings.Default.a_shortavg_percDiff_volt;
            avgYpercPing.Value = Settings.Default.a_shortavg_percDiff_late;

            avgXbaseRefresh.Value = Settings.Default.a_longavg_baseRefresh.TotalMinutes;
            avgXcompareRefresh.Value = Settings.Default.a_longavg_compareRefresh.TotalMinutes;
            avgXlongLimit.Value = Settings.Default.a_longavg_longLimit.TotalMinutes;
            avgXshortLimit.Value = Settings.Default.a_longavg_shortLimit.TotalMinutes;

            avgYbaseRefresh.Value = Settings.Default.a_shortavg_baseRefresh.TotalMinutes;
            avgYcompareRefresh.Value = Settings.Default.a_shortavg_compareRefresh.TotalMinutes;
            avgYlongLimit.Value = Settings.Default.a_shortavg_longLimit.TotalMinutes;
            avgYshortLimit.Value = Settings.Default.a_shortavg_shortLimit.TotalMinutes;

            temperEnabled.IsChecked = Settings.Default.a_enable_temper;
            temperCoeffClear_Clear.Value = Settings.Default.a_temper_coeff_clear_clear;
            temperCoeffClouds_Clear.Value = Settings.Default.a_temper_coeff_clear_cloud;
            temperCoeffAtmo_Clear.Value = Settings.Default.a_temper_coeff_clear_atmos;
            temperCoeffSnow_Clear.Value = Settings.Default.a_temper_coeff_clear_snow;
            temperCoeffRain_Clear.Value = Settings.Default.a_temper_coeff_clear_rain;
            temperCoeffDrizzle_Clear.Value = Settings.Default.a_temper_coeff_clear_drizz;
            temperCoeffStorm_Clear.Value = Settings.Default.a_temper_coeff_clear_storm;

            temperCoeffClear_Clouds.Value = Settings.Default.a_temper_coeff_cloud_clear;
            temperCoeffClouds_Clouds.Value = Settings.Default.a_temper_coeff_cloud_cloud;
            temperCoeffAtmo_Clouds.Value = Settings.Default.a_temper_coeff_cloud_atmos;
            temperCoeffSnow_Clouds.Value = Settings.Default.a_temper_coeff_cloud_snow;
            temperCoeffRain_Clouds.Value = Settings.Default.a_temper_coeff_cloud_rain;
            temperCoeffDrizzle_Clouds.Value = Settings.Default.a_temper_coeff_cloud_drizz;
            temperCoeffStorm_Clouds.Value = Settings.Default.a_temper_coeff_cloud_storm;

            temperDebug.IsChecked = Settings.Default.a_temper_debug;
            temperPerc.Value = Settings.Default.a_temper_percDiff;
            temperDegreesWind.Value = Settings.Default.a_temper_degreesWind;
            temperMaxAge.Value = Settings.Default.a_temper_maxAge.TotalMinutes;
            temperBackDaysCount.Value = Settings.Default.a_temper_backDays;
            temperSkippedDays.Value = Settings.Default.a_temper_skippedDays;
            temperAvgDayCount.Value = Settings.Default.a_temper_averageDays;
            temperMinDiff.Value = Settings.Default.a_temper_minDiff;

            periodEnabled.IsChecked = Settings.Default.a_enable_periodic;
            periodPercSignal.Value = Settings.Default.a_periodic_percDiff_sig;
            periodPercSignalQ.Value = Settings.Default.a_periodic_percDiff_sigQ;
            periodPercVoltage.Value = Settings.Default.a_periodic_percDiff_volt;
            periodDebug.IsChecked = Settings.Default.a_periodic_debug;
        }

        private void SaveFired(object sender, RoutedEventArgs e)
        {
            Settings.Default.a_enable_longavg = (bool)avgXEnabled.IsChecked;
            Settings.Default.a_longavg_percDiff_sig = (float)avgXpercSignal.Value;
            Settings.Default.a_longavg_percDiff_sigQ = (float)avgXpercSignalQ.Value;
            Settings.Default.a_longavg_percDiff_TmpI = (float)avgXpercTempIdu.Value;
            Settings.Default.a_longavg_percDiff_volt = (float)avgXpercVolt.Value;
            Settings.Default.a_longavg_percDiff_late = (float)avgXpercPing.Value;

            Settings.Default.a_enable_shortavg = (bool)avgYEnabled.IsChecked;
            Settings.Default.a_shortavg_percDiff_sig = (float)avgYpercSignal.Value;
            Settings.Default.a_shortavg_percDiff_sigQ = (float)avgYpercSignalQ.Value;
            Settings.Default.a_shortavg_percDiff_TmpI = (float)avgYpercTempIdu.Value;
            Settings.Default.a_shortavg_percDiff_volt = (float)avgYpercVolt.Value;
            Settings.Default.a_shortavg_percDiff_late = (float)avgYpercPing.Value;

            Settings.Default.a_longavg_baseRefresh = TimeSpan.FromMinutes((double)avgXbaseRefresh.Value);
            Settings.Default.a_longavg_compareRefresh = TimeSpan.FromMinutes((double)avgXcompareRefresh.Value);
            Settings.Default.a_longavg_longLimit = TimeSpan.FromMinutes((double)avgXlongLimit.Value);
            Settings.Default.a_longavg_shortLimit = TimeSpan.FromMinutes((double)avgXshortLimit.Value);

            Settings.Default.a_shortavg_baseRefresh = TimeSpan.FromMinutes((double)avgYbaseRefresh.Value);
            Settings.Default.a_shortavg_compareRefresh = TimeSpan.FromMinutes((double)avgYcompareRefresh.Value);
            Settings.Default.a_shortavg_longLimit = TimeSpan.FromMinutes((double)avgYlongLimit.Value);
            Settings.Default.a_shortavg_shortLimit = TimeSpan.FromMinutes((double)avgYshortLimit.Value);

            Settings.Default.a_enable_temper = (bool)temperEnabled.IsChecked;
            Settings.Default.a_temper_coeff_clear_clear = (float)temperCoeffClear_Clear.Value;
            Settings.Default.a_temper_coeff_clear_cloud = (float)temperCoeffClouds_Clear.Value;
            Settings.Default.a_temper_coeff_clear_atmos = (float)temperCoeffAtmo_Clear.Value;
            Settings.Default.a_temper_coeff_clear_snow = (float)temperCoeffSnow_Clear.Value;
            Settings.Default.a_temper_coeff_clear_rain = (float)temperCoeffRain_Clear.Value;
            Settings.Default.a_temper_coeff_clear_drizz = (float)temperCoeffDrizzle_Clear.Value;
            Settings.Default.a_temper_coeff_clear_storm = (float)temperCoeffStorm_Clear.Value;

            Settings.Default.a_temper_coeff_cloud_clear = (float)temperCoeffClear_Clouds.Value;
            Settings.Default.a_temper_coeff_cloud_cloud = (float)temperCoeffClouds_Clouds.Value;
            Settings.Default.a_temper_coeff_cloud_atmos = (float)temperCoeffAtmo_Clouds.Value;
            Settings.Default.a_temper_coeff_cloud_snow = (float)temperCoeffSnow_Clouds.Value;
            Settings.Default.a_temper_coeff_cloud_rain = (float)temperCoeffRain_Clouds.Value;
            Settings.Default.a_temper_coeff_cloud_drizz = (float)temperCoeffDrizzle_Clouds.Value;
            Settings.Default.a_temper_coeff_cloud_storm = (float)temperCoeffStorm_Clouds.Value;

            Settings.Default.a_temper_debug = (bool)temperDebug.IsChecked;
            Settings.Default.a_temper_percDiff = (float)temperPerc.Value;
            Settings.Default.a_temper_degreesWind = (float)temperDegreesWind.Value;
            Settings.Default.a_temper_maxAge = TimeSpan.FromMinutes((double)temperMaxAge.Value);
            Settings.Default.a_temper_backDays = (int)temperBackDaysCount.Value;
            Settings.Default.a_temper_skippedDays = (int)temperSkippedDays.Value;
            Settings.Default.a_temper_averageDays = (int)temperAvgDayCount.Value;
            Settings.Default.a_temper_minDiff = (float)temperMinDiff.Value;

            Settings.Default.a_enable_periodic = (bool)periodEnabled.IsChecked;
            Settings.Default.a_periodic_percDiff_sig = (float)periodPercSignal.Value;
            Settings.Default.a_periodic_percDiff_sigQ = (float)periodPercSignalQ.Value;
            Settings.Default.a_periodic_percDiff_volt = (float)periodPercVoltage.Value;
            Settings.Default.a_periodic_debug = (bool)periodDebug.IsChecked;

            Settings.Default.Save();
            alarmMan.LoadSettings();
            DialogResult = true;
        }

        private void SaveDefaultsFired(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reset();
            Settings.Default.Save();
            alarmMan.LoadSettings();
            DialogResult = true;
        }

        private void LoadDefaultLongAvgFired(object sender, RoutedEventArgs e)
        {
            avgXEnabled.IsChecked = Boolean.Parse((string)Settings.Default.Properties["a_enable_longavg"].DefaultValue);
            avgXpercSignal.Value = Double.Parse((string)Settings.Default.Properties["a_longavg_percDiff_sig"].DefaultValue, CultureInfo.InvariantCulture);
            avgXpercSignalQ.Value = Double.Parse((string)Settings.Default.Properties["a_longavg_percDiff_sigQ"].DefaultValue, CultureInfo.InvariantCulture);
            avgXpercTempIdu.Value = Double.Parse((string)Settings.Default.Properties["a_longavg_percDiff_TmpI"].DefaultValue, CultureInfo.InvariantCulture);
            avgXpercVolt.Value = Double.Parse((string)Settings.Default.Properties["a_longavg_percDiff_volt"].DefaultValue, CultureInfo.InvariantCulture);
            avgXpercPing.Value = Double.Parse((string)Settings.Default.Properties["a_longavg_percDiff_late"].DefaultValue, CultureInfo.InvariantCulture);

            avgXbaseRefresh.Value = TimeSpan.Parse((string)Settings.Default.Properties["a_longavg_baseRefresh"].DefaultValue, CultureInfo.InvariantCulture).TotalMinutes;
            avgXcompareRefresh.Value = TimeSpan.Parse((string)Settings.Default.Properties["a_longavg_compareRefresh"].DefaultValue, CultureInfo.InvariantCulture).TotalMinutes;
            avgXlongLimit.Value = TimeSpan.Parse((string)Settings.Default.Properties["a_longavg_longLimit"].DefaultValue, CultureInfo.InvariantCulture).TotalMinutes;
            avgXshortLimit.Value = TimeSpan.Parse((string)Settings.Default.Properties["a_longavg_shortLimit"].DefaultValue, CultureInfo.InvariantCulture).TotalMinutes;
        }

        private void LoadDefaultShortAvgFired(object sender, RoutedEventArgs e)
        {
            avgYEnabled.IsChecked = Boolean.Parse((string)Settings.Default.Properties["a_enable_shortavg"].DefaultValue);
            avgYpercSignal.Value = Double.Parse((string)Settings.Default.Properties["a_shortavg_percDiff_sig"].DefaultValue, CultureInfo.InvariantCulture);
            avgYpercSignalQ.Value = Double.Parse((string)Settings.Default.Properties["a_shortavg_percDiff_sigQ"].DefaultValue, CultureInfo.InvariantCulture);
            avgYpercTempIdu.Value = Double.Parse((string)Settings.Default.Properties["a_shortavg_percDiff_TmpI"].DefaultValue, CultureInfo.InvariantCulture);
            avgYpercVolt.Value = Double.Parse((string)Settings.Default.Properties["a_shortavg_percDiff_volt"].DefaultValue, CultureInfo.InvariantCulture);
            avgYpercPing.Value = Double.Parse((string)Settings.Default.Properties["a_shortavg_percDiff_late"].DefaultValue, CultureInfo.InvariantCulture);

            avgYbaseRefresh.Value = TimeSpan.Parse((string)Settings.Default.Properties["a_shortavg_baseRefresh"].DefaultValue, CultureInfo.InvariantCulture).TotalMinutes;
            avgYcompareRefresh.Value = TimeSpan.Parse((string)Settings.Default.Properties["a_shortavg_compareRefresh"].DefaultValue, CultureInfo.InvariantCulture).TotalMinutes;
            avgYlongLimit.Value = TimeSpan.Parse((string)Settings.Default.Properties["a_shortavg_longLimit"].DefaultValue, CultureInfo.InvariantCulture).TotalMinutes;
            avgYshortLimit.Value = TimeSpan.Parse((string)Settings.Default.Properties["a_shortavg_shortLimit"].DefaultValue, CultureInfo.InvariantCulture).TotalMinutes;
        }

        private void LoadDefaultTemperFired(object sender, RoutedEventArgs e)
        {
            temperEnabled.IsChecked = Boolean.Parse((string)Settings.Default.Properties["a_enable_temper"].DefaultValue);
            temperCoeffClear_Clear.Value = Double.Parse((string)Settings.Default.Properties["a_temper_coeff_clear_clear"].DefaultValue, CultureInfo.InvariantCulture);
            temperCoeffClouds_Clear.Value = Double.Parse((string)Settings.Default.Properties["a_temper_coeff_clear_cloud"].DefaultValue, CultureInfo.InvariantCulture);
            temperCoeffAtmo_Clear.Value = Double.Parse((string)Settings.Default.Properties["a_temper_coeff_clear_atmos"].DefaultValue, CultureInfo.InvariantCulture);
            temperCoeffSnow_Clear.Value = Double.Parse((string)Settings.Default.Properties["a_temper_coeff_clear_snow"].DefaultValue, CultureInfo.InvariantCulture);
            temperCoeffRain_Clear.Value = Double.Parse((string)Settings.Default.Properties["a_temper_coeff_clear_rain"].DefaultValue, CultureInfo.InvariantCulture);
            temperCoeffDrizzle_Clear.Value = Double.Parse((string)Settings.Default.Properties["a_temper_coeff_clear_drizz"].DefaultValue, CultureInfo.InvariantCulture);
            temperCoeffStorm_Clear.Value = Double.Parse((string)Settings.Default.Properties["a_temper_coeff_clear_storm"].DefaultValue, CultureInfo.InvariantCulture);

            temperCoeffClear_Clouds.Value = Double.Parse((string)Settings.Default.Properties["a_temper_coeff_cloud_clear"].DefaultValue, CultureInfo.InvariantCulture);
            temperCoeffClouds_Clouds.Value = Double.Parse((string)Settings.Default.Properties["a_temper_coeff_cloud_cloud"].DefaultValue, CultureInfo.InvariantCulture);
            temperCoeffAtmo_Clouds.Value = Double.Parse((string)Settings.Default.Properties["a_temper_coeff_cloud_atmos"].DefaultValue, CultureInfo.InvariantCulture);
            temperCoeffSnow_Clouds.Value = Double.Parse((string)Settings.Default.Properties["a_temper_coeff_cloud_snow"].DefaultValue, CultureInfo.InvariantCulture);
            temperCoeffRain_Clouds.Value = Double.Parse((string)Settings.Default.Properties["a_temper_coeff_cloud_rain"].DefaultValue, CultureInfo.InvariantCulture);
            temperCoeffDrizzle_Clouds.Value = Double.Parse((string)Settings.Default.Properties["a_temper_coeff_cloud_drizz"].DefaultValue, CultureInfo.InvariantCulture);
            temperCoeffStorm_Clouds.Value = Double.Parse((string)Settings.Default.Properties["a_temper_coeff_cloud_storm"].DefaultValue, CultureInfo.InvariantCulture);

            temperDebug.IsChecked = Boolean.Parse((string)Settings.Default.Properties["a_temper_debug"].DefaultValue);
            temperPerc.Value = Double.Parse((string)Settings.Default.Properties["a_temper_percDiff"].DefaultValue, CultureInfo.InvariantCulture);
            temperDegreesWind.Value = Double.Parse((string)Settings.Default.Properties["a_temper_degreesWind"].DefaultValue, CultureInfo.InvariantCulture);
            temperMaxAge.Value = TimeSpan.Parse((string)Settings.Default.Properties["a_temper_maxAge"].DefaultValue, CultureInfo.InvariantCulture).TotalMinutes;
            temperBackDaysCount.Value = Double.Parse((string)Settings.Default.Properties["a_temper_backDays"].DefaultValue, CultureInfo.InvariantCulture);
            temperSkippedDays.Value = Double.Parse((string)Settings.Default.Properties["a_temper_skippedDays"].DefaultValue, CultureInfo.InvariantCulture);
            temperAvgDayCount.Value = Double.Parse((string)Settings.Default.Properties["a_temper_averageDays"].DefaultValue, CultureInfo.InvariantCulture);
            temperMinDiff.Value = Double.Parse((string)Settings.Default.Properties["a_temper_minDiff"].DefaultValue, CultureInfo.InvariantCulture);
        }

        private void LoadDefaultPeriodFired(object sender, RoutedEventArgs e)
        {
            periodEnabled.IsChecked = Boolean.Parse((string)Settings.Default.Properties["a_enable_periodic"].DefaultValue);
            periodPercSignal.Value = Double.Parse((string)Settings.Default.Properties["a_periodic_percDiff_sig"].DefaultValue, CultureInfo.InvariantCulture);
            periodPercSignalQ.Value = Double.Parse((string)Settings.Default.Properties["a_periodic_percDiff_sigQ"].DefaultValue, CultureInfo.InvariantCulture);
            periodPercVoltage.Value = Double.Parse((string)Settings.Default.Properties["a_periodic_percDiff_volt"].DefaultValue, CultureInfo.InvariantCulture);
            periodDebug.IsChecked = Boolean.Parse((string)Settings.Default.Properties["a_periodic_debug"].DefaultValue);
        }
    }
}
