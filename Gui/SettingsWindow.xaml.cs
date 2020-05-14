﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
using MicrowaveMonitor.Managers;
using MicrowaveMonitor.Properties;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using MicrowaveMonitor.Database;
using System.ComponentModel;

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
            avgXpercSignal.Value = Settings.Default.a_longavg_percDiff_sig;
            avgXpercSignalQ.Value = Settings.Default.a_longavg_percDiff_sigQ;
            avgXpercTempIdu.Value = Settings.Default.a_longavg_percDiff_TmpI;
            avgXpercVolt.Value = Settings.Default.a_longavg_percDiff_volt;
            avgXpercPing.Value = Settings.Default.a_longavg_percDiff_late;

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
        }

        private void SaveFired(object sender, RoutedEventArgs e)
        {
            Settings.Default.a_longavg_percDiff_sig = (float)avgXpercSignal.Value;
            Settings.Default.a_longavg_percDiff_sigQ = (float)avgXpercSignalQ.Value;
            Settings.Default.a_longavg_percDiff_TmpI = (float)avgXpercTempIdu.Value;
            Settings.Default.a_longavg_percDiff_volt = (float)avgXpercVolt.Value;
            Settings.Default.a_longavg_percDiff_late = (float)avgXpercPing.Value;

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
        }
    }
}
