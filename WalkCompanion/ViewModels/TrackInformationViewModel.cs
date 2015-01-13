using WalkCompanion.Commands;
using WalkCompanion.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace WalkCompanion.ViewModels
{
    public class TrackInformationViewModel : INotifyPropertyChanged
    {
        public Guid? LoadRouteId { get; set; }
        public bool KickNewSession { get; set; }
        public Tracker Tracer { get; private set; }

        #region Commands

        public ICommand StartSession { get; private set; }
        public ICommand PauseSession { get; private set; }
        public ICommand StopSession { get; private set; }
        public ICommand StoreSession { get; private set; }
        public ICommand LoadSession { get; private set; }
        public ICommand PickSession { get; private set; }
        public ICommand LoadSettings { get; private set; }
        public ICommand SaveSettings { get; private set; }

        #endregion

        public TrackInformationViewModel()
        {
            StartSession = new StartSession(this);
            PauseSession = new PauseSession(this);
            StopSession = new StopSession(this);
            StoreSession = new StoreSession(this);
            LoadSession = new LoadSession(this);
            PickSession = new PickSession(this);
            LoadSettings = new LoadSettings(this);
            SaveSettings = new SaveSettings(this);

            Tracer = new Tracker(this);

            KickNewSession = false;

            ResetLayout();
        }

        public void ResetLayout()
        {
            StartScreenVisibility = Visibility.Visible;
            StartBtnVisibility = Visibility.Visible;
            PauseBtnVisibility = Visibility.Collapsed;
            StopBtnVisibility = Visibility.Collapsed;
            StartBtnEnabled = false;
            ShowInfoPanelVisibility = Visibility.Collapsed;
            EndWalkOptionsVisibility = Visibility.Collapsed;
            LoadingMapVisibility = Visibility.Visible;
            LostConnectionVisibility = Visibility.Collapsed;

            TotalDistanceTravelled = "";
            TimeElapsed = "";
            TimeStarted = "";
            CaloriesBurned = "";
            AverageSpeed = "";
        }

        #region UI modifier events

        public void StartSessionUiNotifier()
        {
            StartScreenVisibility = Visibility.Collapsed;
            StartBtnVisibility = Visibility.Collapsed;
            PauseBtnVisibility = Visibility.Visible;
            StopBtnVisibility = Visibility.Visible;
            ShowInfoPanelVisibility = Visibility.Visible;

            ExecuteUICommand = "ClearMap";
        }

        public void StopSessionUiNotifier()
        {
            StartBtnVisibility = Visibility.Visible;
            PauseBtnVisibility = Visibility.Collapsed;
            StopBtnVisibility = Visibility.Collapsed;
            EndWalkOptionsVisibility = Visibility.Visible;

            ExecuteUICommand = "StoppinSession";
        }

        #endregion

        private string _executeUICommand;
        public string ExecuteUICommand
        {
            get { return _executeUICommand; }
            set
            {
                _executeUICommand = value;
                NotifyPropertyChanged("ExecuteUICommand");
            }
        }

        #region ViewModel Properties

        private string _totalDistanceTravelled;
        public string TotalDistanceTravelled
        {
            get { return _totalDistanceTravelled; }
            set
            {
                if (value != _totalDistanceTravelled)
                {
                    _totalDistanceTravelled = value;
                    NotifyPropertyChanged("TotalDistanceTravelled");
                }
            }
        }

        public string _timeElapsed;
        public string TimeElapsed
        {
            get { return _timeElapsed; }
            set
            {
                if (value != _timeElapsed)
                {
                    _timeElapsed = value;
                    NotifyPropertyChanged("TimeElapsed");
                }
            }
        }

        public string _timeStarted;
        public string TimeStarted
        {
            get { return _timeStarted; }
            set
            {
                if (value != _timeStarted)
                {
                    _timeStarted = value;
                    NotifyPropertyChanged("TimeStarted");
                }
            }
        }

        public string _timeFinished;
        public string TimeFinished
        {
            get { return _timeFinished; }
            set
            {
                if (value != _timeFinished)
                {
                    _timeFinished = value;
                    NotifyPropertyChanged("TimeFinished");
                }
            }
        }

        private string _caloriesBurned;
        public string CaloriesBurned
        {
            get { return _caloriesBurned; }
            set
            {
                if (value != _caloriesBurned)
                {
                    _caloriesBurned = value;
                    NotifyPropertyChanged("CaloriesBurned");
                }
            }
        }

        private string _averageSpeed;
        public string AverageSpeed
        {
            get { return _averageSpeed; }
            set
            {
                if (value != _averageSpeed)
                {
                    _averageSpeed = value;
                    NotifyPropertyChanged("AverageSpeed");
                }
            }
        }

        private Visibility _loadingInitialPosition;
        public Visibility LoadingInitialPosition
        {
            get { return _loadingInitialPosition; }
            set
            {
                if (value != _loadingInitialPosition)
                {
                    _loadingInitialPosition = value;
                    NotifyPropertyChanged("LoadingInitialPosition");
                }
            }
        }

        private Visibility _lostConnectionVisibility;
        public Visibility LostConnectionVisibility
        {
            get { return _lostConnectionVisibility; }
            set
            {
                if (value != _lostConnectionVisibility)
                {
                    _lostConnectionVisibility = value;
                    NotifyPropertyChanged("LostConnectionVisibility");
                }
            }
        }

        private Visibility _startScreenVisibility;
        public Visibility StartScreenVisibility
        {
            get { return _startScreenVisibility; }
            set
            {
                if (value != _startScreenVisibility)
                {
                    _startScreenVisibility = value;
                    NotifyPropertyChanged("StartScreenVisibility");
                }
            }
        }

        private Visibility _startBtnVisibility;
        public Visibility StartBtnVisibility
        {
            get { return _startBtnVisibility; }
            set
            {
                if (value != _startBtnVisibility)
                {
                    _startBtnVisibility = value;
                    NotifyPropertyChanged("StartBtnVisibility");
                }
            }
        }

        private Visibility _pauseBtnVisibility;
        public Visibility PauseBtnVisibility
        {
            get { return _pauseBtnVisibility; }
            set
            {
                if (value != _pauseBtnVisibility)
                {
                    _pauseBtnVisibility = value;
                    NotifyPropertyChanged("PauseBtnVisibility");
                }
            }
        }

        private Visibility _stopBtnVisibility;
        public Visibility StopBtnVisibility
        {
            get { return _stopBtnVisibility; }
            set
            {
                if (value != _stopBtnVisibility)
                {
                    _stopBtnVisibility = value;
                    NotifyPropertyChanged("StopBtnVisibility");
                }
            }
        }

        private bool _startBtnEnabled;
        public bool StartBtnEnabled
        {
            get { return _startBtnEnabled; }
            set
            {
                if (value != _startBtnEnabled)
                {
                    _startBtnEnabled = value;
                    NotifyPropertyChanged("StartBtnEnabled");
                }
            }
        }

        private Visibility _showInfoPanelVisibility;
        public Visibility ShowInfoPanelVisibility
        {
            get { return _showInfoPanelVisibility; }
            set
            {
                if (value != _showInfoPanelVisibility)
                {
                    _showInfoPanelVisibility = value;
                    NotifyPropertyChanged("ShowInfoPanelVisibility");
                }
            }
        }

        private Visibility _endWalkOptionsVisibility;
        public Visibility EndWalkOptionsVisibility
        {
            get { return _endWalkOptionsVisibility; }
            set
            {
                if (value != _endWalkOptionsVisibility)
                {
                    _endWalkOptionsVisibility = value;
                    NotifyPropertyChanged("EndWalkOptionsVisibility");
                }
            }
        }

        private Visibility _loadingMapVisibility;
        public Visibility LoadingMapVisibility
        {
            get { return _loadingMapVisibility; }
            set
            {
                if (value != _loadingMapVisibility)
                {
                    _loadingMapVisibility = value;
                    NotifyPropertyChanged("LoadingMapVisibility");
                }
            }
        }

        private ObservableCollection<VisualStoredRoute> _visualStoredRoute;
        public ObservableCollection<VisualStoredRoute> VisualStoredRoutes
        {
            get
            {
                if (_visualStoredRoute == null)
                    Tracer.VisualStoredRoutes_CollectionChanged(null, null);

                return _visualStoredRoute;
            }
            set
            {
                if (value != _visualStoredRoute)
                {
                    _visualStoredRoute = value;
                    NotifyPropertyChanged("VisualStoredRoutes");
                }

            }
        }

#endregion

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}