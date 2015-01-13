using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace WalkCompanion.Controls
{
    public partial class WalkDetailsControl : UserControl
    {
        public WalkDetailsControl()
        {
            InitializeComponent();

            Loaded += WalkDetailsControl_Loaded;
        }

        void WalkDetailsControl_Loaded(object sender, RoutedEventArgs e)
        {
            //bool useMetricSystem = App.UserSavedSettings.UseMetricSystem;

            //ShowMeters.Visibility = useMetricSystem ? Visibility.Visible : Visibility.Collapsed;
            //ShowYards.Visibility = !useMetricSystem ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
