using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Services;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Device.Location;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WalkCompanion.Models;
using WalkCompanion.Resources;

namespace WalkCompanion
{
    public partial class MainPage : PhoneApplicationPage
    {
        private bool settingsWindowOpen = false;
        private bool pickSessionWindowOpen = false;

        public MainPage()
        {
            InitializeComponent();

            DataContext = App.TrackInformationViewModel;

            BuildLocalizedApplicationBar();

            UseMetricSystemCheckbox.IsChecked = App.UserSavedSettings.UseMetricSystem;

            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.TrackInformationViewModel.Tracer.GetCurrentCoordinate();

            App.TrackInformationViewModel.KickNewSession = false;

            LoadStoredRoutes();

            if (App.TrackInformationViewModel.Tracer.UnexpectedSessionTermination)
            {
                LogPanel.Visibility = Visibility.Visible;
                App.TrackInformationViewModel.Tracer.UnexpectedSessionTermination = false;
            }
        }

        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarMenuItem appBarAboutMenuItem = new ApplicationBarMenuItem(AppResources.About);
            appBarAboutMenuItem.Click += appBarAboutMenuItem_Click;
            ApplicationBar.MenuItems.Add(appBarAboutMenuItem);

            ApplicationBar.Mode = ApplicationBarMode.Minimized;
        }

        void appBarAboutMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (App.TrackInformationViewModel.Tracer.SessionUnderway == false)
                App.TrackInformationViewModel.ResetLayout();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (App.TrackInformationViewModel.Tracer.UnexpectedSessionTermination)
                LogPanel.Visibility = Visibility.Collapsed;
        }

        private void LoadStoredRoutes()
        {
            App.TrackInformationViewModel.VisualStoredRoutes.CollectionChanged += StoredRoutes_CollectionChanged;

            PickSessionSelector.SelectedItem = null;

            StoredRoutes_CollectionChanged(null, null);
        }
        private void StoredRoutes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NoStoredWalksMessage.Visibility = App.TrackInformationViewModel.Tracer.StoredRoutes.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        //Deactives Running in background when:
        //  any events of Geolocator will be removed
        //  GeoCoordinateWatcher.Stop()
        //  Batter Saver is active
        //  low mem
        //  users disabled location services
        //  another app begins running in background
        //  runs in background without interaction for 4hours

        //  TRYOUT
        private ReverseGeocodeQuery MyReverseGeocodeQuery = null;

        private void DrawMapMarker(GeoCoordinate coordinate, Color color, MapLayer mapLayer)
        {
            // Create a map marker
            Polygon polygon = new Polygon();
            polygon.Points.Add(new Point(0, 0));
            polygon.Points.Add(new Point(0, 75));
            polygon.Points.Add(new Point(25, 0));
            polygon.Fill = new SolidColorBrush(color);

            // Enable marker to be tapped for location information
            polygon.Tag = new GeoCoordinate(coordinate.Latitude, coordinate.Longitude);
            polygon.MouseLeftButtonUp += new MouseButtonEventHandler(Marker_Click);

            // Create a MapOverlay and add marker
            MapOverlay overlay = new MapOverlay();
            overlay.Content = polygon;
            overlay.GeoCoordinate = new GeoCoordinate(coordinate.Latitude, coordinate.Longitude);
            overlay.PositionOrigin = new Point(0.0, 1.0);
            mapLayer.Add(overlay);
        }

        private void Marker_Click(object sender, EventArgs e)
        {
            Polygon p = (Polygon)sender;
            GeoCoordinate geoCoordinate = (GeoCoordinate)p.Tag;
            MyReverseGeocodeQuery = new ReverseGeocodeQuery();
            MyReverseGeocodeQuery.GeoCoordinate = new GeoCoordinate(geoCoordinate.Latitude, geoCoordinate.Longitude);
            MyReverseGeocodeQuery.QueryCompleted += ReverseGeocodeQuery_QueryCompleted;
            MyReverseGeocodeQuery.QueryAsync();
        }

        private void ReverseGeocodeQuery_QueryCompleted(object sender, QueryCompletedEventArgs<IList<MapLocation>> e)
        {
            if (e.Error == null)
            {
                if (e.Result.Count > 0)
                {
                    MapAddress address = e.Result[0].Information.Address;
                    String msgBoxText = "";

                    if (address.Country.Length > 0) msgBoxText += "\n" + address.Country;
                    MessageBox.Show(msgBoxText, "Map Explorer", MessageBoxButton.OK);
                }
            }
        }

        private void OpenSettings_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            settingsWindowOpen = true;

            SettingsFlyout.Visibility = Visibility.Visible;
        }

        private void CloseSettings_Click(object sender, RoutedEventArgs e)
        {
            settingsWindowOpen = false;

            SettingsFlyout.Visibility = Visibility.Collapsed;
        }

        private void StartBtn_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            App.TrackInformationViewModel.KickNewSession = true;
            NavigationService.Navigate(new Uri("/NavigationPage.xaml", UriKind.Relative));
        }

        private void PickSession_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            pickSessionWindowOpen = true;

            PickSessionFlyout.Visibility = Visibility.Visible;
        }

        private void ClosePickSession_Click(object sender, RoutedEventArgs e)
        {
            pickSessionWindowOpen = false;

            PickSessionFlyout.Visibility = Visibility.Collapsed;

            PickSessionSelector.SelectedItem = null;
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (settingsWindowOpen == true)
            {
                e.Cancel = true;

                CloseSettings_Click(null, null);
            }

            if (pickSessionWindowOpen == true)
            {
                e.Cancel = true;

                ClosePickSession_Click(null, null);
            }
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            App.TrackInformationViewModel.Tracer.StoredRoutes.CollectionChanged -= StoredRoutes_CollectionChanged;

            ClosePickSession_Click(null, null);
            CloseSettings_Click(null, null);
        }

        private void PickSession_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0] != null)
            {
                Guid selectedRouteId = ((VisualStoredRoute)e.AddedItems[0]).Id;

                App.TrackInformationViewModel.LoadRouteId = selectedRouteId;
                StartBtn_Tap(null, null);
            }
        }

        private void UseMetricSystem_Unchecked(object sender, RoutedEventArgs e)
        {
            App.TrackInformationViewModel.Tracer.UseMetricSystem = false;
        }

        private void UseMetricSystem_Checked(object sender, RoutedEventArgs e)
        {
            App.TrackInformationViewModel.Tracer.UseMetricSystem = true;
        }

    }
}