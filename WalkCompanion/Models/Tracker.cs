using WalkCompanion.Resources;
using WalkCompanion.Utils;
using WalkCompanion.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Device.Location;
using System.Windows;
using System.Windows.Threading;
using Windows.Devices.Geolocation;

namespace WalkCompanion.Models
{
    public class Tracker
    {
        #region Fields & Constructor

        private const string ROUTES_STORAGE_KEY = "STORED_ROUTES";

        private const int STORE_COORDINATE_MIN_DISTANCE = 10;    //unit in meters
        private const int MOVEMENT_THRESHOLD = 2;    //unit in meters
        private const double TIMER_UPDATE_INTERVAL = 0.7;   //unit in sec

        public GeoCoordinate CurrentCoordinate { get; set; }
        public GeoCoordinate PreviousCoordinate { get; set; }

        public bool UnexpectedSessionTermination { get; set; }

        public List<GeoCoordinate> StoredCoordinates { get; set; }
        private GeoCoordinate lastStoredCoordinate;

        private TrackInformationViewModel _viewModel;

        private double accuracy = 0.0;

        public double CurrentSessionDistanceTravelled { get; private set; }
        public DateTime CurrentSessionStartTime { get; set; }
        public DateTime CurrentSessionEndTime { get; set; }
        public TimeSpan CurrentSessionTimeElapsed { get; set; }
        public DispatcherTimer Timer { get; set; }

        private Geolocator _locator;
        public Geolocator Locator
        {
            get
            {
                if (_locator == null)
                {
                    _locator = new Geolocator();
                    _locator.DesiredAccuracy = PositionAccuracy.High;
                }

                return _locator;
            }
            set
            {
                _locator = value;
            }
        }
        public bool SessionUnderway { get; set; }
        public bool SessionPaused { get; set; }

        private ObservableCollection<StoredRoute> _storedRoutes;
        public ObservableCollection<StoredRoute> StoredRoutes
        {
            get
            {
                if (_storedRoutes == null)
                {
                    ObservableCollection<StoredRoute> result;
                    Storage.TryGetSetting<ObservableCollection<StoredRoute>>(ROUTES_STORAGE_KEY, out result);

                    _storedRoutes = result ?? new ObservableCollection<StoredRoute>();
                }

                return _storedRoutes;
            }
            set
            {
                _storedRoutes = value;

                ForceSaveRoutes();
            }
        }

        public bool UseMetricSystem
        {
            get
            {
                return App.UserSavedSettings.UseMetricSystem;
            }
            set
            {
                App.UserSavedSettings.UseMetricSystem = value;

                //external dependency
                VisualStoredRoutes_CollectionChanged(null, null);

                App.SaveUserSettings();
            }
        }

        public List<GeoCoordinate> CoordinatesGotOffline { get; set; }

        public Tracker(TrackInformationViewModel viewModel)
        {
            _viewModel = viewModel;

            StoredCoordinates = new List<GeoCoordinate>();
            CurrentSessionDistanceTravelled = 0;

            StoredRoutes.CollectionChanged += VisualStoredRoutes_CollectionChanged;

            CoordinatesGotOffline = new List<GeoCoordinate>();
        }

        #endregion

        public void VisualStoredRoutes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            List<VisualStoredRoute> tempVisualStoredRoutes = new List<VisualStoredRoute>();

            foreach (StoredRoute route in StoredRoutes)
            {
                string distanceConverted = UnitsConverter.TransformMetersToDesiredUnit(route.DistanceTravelled, UseMetricSystem);

                VisualStoredRoute visualRoute = new VisualStoredRoute
                {
                    Id = route.Id,
                    Name = route.Name,
                    StartDate = UnitsConverter.DateTimeToString(route.StartDate),
                    EndDate = UnitsConverter.DateTimeToString(route.EndDate),
                    DistanceTravelled = distanceConverted,
                    TimeTravelled = UnitsConverter.TimeSpanToString(route.TimeTravelled)
                };

                tempVisualStoredRoutes.Add(visualRoute);
            }

            _viewModel.VisualStoredRoutes = new ObservableCollection<VisualStoredRoute>(tempVisualStoredRoutes);
        }

        private void StoreCoordinate(GeoCoordinate currentCoordinate)
        {
            if (lastStoredCoordinate == null || lastStoredCoordinate.GetDistanceTo(currentCoordinate) > STORE_COORDINATE_MIN_DISTANCE)
            {
                StoredCoordinates.Add(currentCoordinate);

                lastStoredCoordinate = currentCoordinate;

                _viewModel.ExecuteUICommand = "AddPointToMap";
                _viewModel.ExecuteUICommand = "CenterMap";
            }
        }

        private GeoCoordinate CastGeocoordinate(Geocoordinate newCoordinate)
        {
            return newCoordinate.Altitude.HasValue ?
                            new GeoCoordinate(newCoordinate.Latitude, newCoordinate.Longitude, newCoordinate.Altitude.Value) :
                            new GeoCoordinate(newCoordinate.Latitude, newCoordinate.Longitude);
        }

        private void ForceSaveRoutes()
        {
            Storage.StoreSetting<ObservableCollection<StoredRoute>>(ROUTES_STORAGE_KEY, StoredRoutes);
        }

        private void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (!SessionPaused)
                {
                    AddNewEntry(args.Position.Coordinate);

                    CurrentSessionDistanceTravelled += (PreviousCoordinate != null ? CurrentCoordinate.GetDistanceTo(PreviousCoordinate) : 0);

                    _viewModel.TotalDistanceTravelled = UnitsConverter.TransformMetersToDesiredUnit(CurrentSessionDistanceTravelled, UseMetricSystem);
                    _viewModel.CaloriesBurned = UnitsConverter.RoundDoubleAndConvertToString(Calculators.CaloriesBurned(CurrentSessionDistanceTravelled, CurrentSessionTimeElapsed));
                    _viewModel.AverageSpeed = UnitsConverter.TransformSpeedToDesiredUnit(Calculators.AverageSpeed(CurrentSessionDistanceTravelled, CurrentSessionTimeElapsed), UseMetricSystem);

                    _viewModel.LoadingMapVisibility = Visibility.Collapsed;
                }
            });
        }

        public void StartNewSession()
        {
            SessionUnderway = true;
            SessionPaused = false;

            Locator.MovementThreshold = MOVEMENT_THRESHOLD;
            Locator.PositionChanged += Geolocator_PositionChanged;

            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromSeconds(TIMER_UPDATE_INTERVAL);
            Timer.Tick += OnTimerTick;
            Timer.Start();

            lastStoredCoordinate = null;
            StoredCoordinates.Clear();

            CurrentSessionStartTime = DateTime.Now;
            CurrentSessionDistanceTravelled = 0;

            _viewModel.TimeStarted = UnitsConverter.DateTimeToString(CurrentSessionStartTime);
            _viewModel.TotalDistanceTravelled = UnitsConverter.TransformMetersToDesiredUnit(0, UseMetricSystem);

            _viewModel.StartSessionUiNotifier();
        }

        public void LoadSession()
        {
            SessionUnderway = true;
            SessionPaused = false;

            Locator.MovementThreshold = MOVEMENT_THRESHOLD;
            Locator.PositionChanged += Geolocator_PositionChanged;

            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromSeconds(TIMER_UPDATE_INTERVAL);
            Timer.Tick += OnTimerTick;
            Timer.Start();

            lastStoredCoordinate = null;
            StoredCoordinates.Clear();

            CurrentSessionStartTime = DateTime.Now;
            CurrentSessionDistanceTravelled = 0;

            _viewModel.TimeStarted = UnitsConverter.DateTimeToString(CurrentSessionStartTime);
            _viewModel.TotalDistanceTravelled = UnitsConverter.TransformMetersToDesiredUnit(0, UseMetricSystem);

            _viewModel.StartSessionUiNotifier();
        }

        public void StopCurrentSession()
        {
            SessionUnderway = false;
            SessionPaused = false;

            Locator.PositionChanged -= Geolocator_PositionChanged;
            Locator = null;

            Timer.Tick -= OnTimerTick;

            CurrentSessionEndTime = DateTime.Now;

            _viewModel.TimeFinished = UnitsConverter.DateTimeToString(CurrentSessionEndTime);
            _viewModel.StopSessionUiNotifier();

            TileUpdate.ClearTile();

            CoordinatesGotOffline.Clear();
        }

        public void PauseCurrentSession()
        {
            SessionPaused = true;

            _viewModel.PauseBtnVisibility = Visibility.Collapsed;
        }

        public void ResumeCurrentSession()
        {
            SessionPaused = false;

            _viewModel.PauseBtnVisibility = Visibility.Visible;
        }

        public void StoreCurrentSession()
        {
            StoredRoute route = new StoredRoute
            {
                Id = Guid.NewGuid(),
                StartDate = CurrentSessionStartTime,
                EndDate = CurrentSessionEndTime,
                DistanceTravelled = CurrentSessionDistanceTravelled,
                StoredCoordinates = new List<GeoCoordinate>(),
                TimeTravelled = CurrentSessionTimeElapsed
            };
            route.StoredCoordinates.AddRange(StoredCoordinates);

            StoredRoutes.Add(route);
            ForceSaveRoutes();

            _viewModel.ExecuteUICommand = "RouteStored";

            TileUpdate.UpdateIconicTile(AppResources.ApplicationTitle, 0, AppResources.PreviousWalk, string.Format("{0}: {1}", AppResources.TotalDistance, _viewModel.TotalDistanceTravelled), string.Format("{0}: {1}", AppResources.TotalTime, _viewModel.TimeElapsed));
        }

        public ObservableCollection<StoredRoute> PickSession()
        {
            ObservableCollection<StoredRoute> result;
            Storage.TryGetSetting<ObservableCollection<StoredRoute>>(ROUTES_STORAGE_KEY, out result);

            return result;
        }

        public void AddNewEntry(Geocoordinate newCoordinate)
        {
            PreviousCoordinate = CurrentCoordinate;

            CurrentCoordinate = CastGeocoordinate(newCoordinate);

            StoreCoordinate(CurrentCoordinate);

            if (App.RunningInBackground)
            {
                TileUpdate.UpdateIconicTile(AppResources.ApplicationTitle, 1, AppResources.RecordingWalk, string.Format("{0}: {1}", AppResources.DistanceTravelled, _viewModel.TotalDistanceTravelled), string.Format("{0}: {1}", AppResources.TimeElapsed, _viewModel.TimeElapsed));

                CoordinatesGotOffline.Add(CurrentCoordinate);
            }
        }

        public void OnTimerTick(object sender, EventArgs args)
        {
            if (!SessionPaused)
            {
                CurrentSessionTimeElapsed = DateTime.Now.Subtract(CurrentSessionStartTime);

                _viewModel.TimeElapsed = UnitsConverter.TimeSpanToString(CurrentSessionTimeElapsed);
            }
        }

        public async void GetCurrentCoordinate()
        {
            App.TrackInformationViewModel.LoadingInitialPosition = Visibility.Visible;

            try
            {
                Geoposition currentPosition = await Locator.GetGeopositionAsync(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(10));
                accuracy = currentPosition.Coordinate.Accuracy;

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //CurrentCoordinate = new GeoCoordinate(currentPosition.Coordinate.Latitude, currentPosition.Coordinate.Longitude);

                    App.TrackInformationViewModel.StartBtnEnabled = true;

                    App.TrackInformationViewModel.LoadingInitialPosition = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                App.HandleControlledExceptions(ex);

                App.TrackInformationViewModel.StartBtnEnabled = false;
                App.TrackInformationViewModel.LostConnectionVisibility = Visibility.Visible;

                App.TrackInformationViewModel.LoadingInitialPosition = Visibility.Collapsed;
            }
        }
    }
}
