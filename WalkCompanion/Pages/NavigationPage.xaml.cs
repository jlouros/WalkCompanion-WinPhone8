using WalkCompanion.Controls;
using WalkCompanion.Models;
using WalkCompanion.Resources;
using WalkCompanion.Utils;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WalkCompanion
{
    public partial class NavigationPage : PhoneApplicationPage
    {
        #region Properties & Fields

        private const int DEFAULT_ZOOM_LEVEL = 20;
        private bool mapViewFollow;
        private StoredRoute loadedRoute;
        private bool goingBackToMainScreen;

        private MapPolyline _currentPolyline;
        private MapPolyline currentPolyline
        {
            get
            {
                if (_currentPolyline == null)
                {
                    _currentPolyline = new MapPolyline();

                    currentPolyline.StrokeColor = Color.FromArgb(0x80, 0x00, 0xFF, 0x00);
                    currentPolyline.StrokeThickness = 5;

                    MyMap.MapElements.Add(currentPolyline);
                }

                return _currentPolyline;
            }
        }

        private List<GeoCoordinate> _drawnPoints;
        private List<GeoCoordinate> DrawnPoints
        {
            get
            {
                if (_drawnPoints == null)
                    _drawnPoints = new List<GeoCoordinate>();

                return _drawnPoints;
            }
        }

        #endregion

        public NavigationPage()
        {
            InitializeComponent();

            BuildLocalizedApplicationBar();

            DataContext = App.TrackInformationViewModel;

            Loaded += FreshStart_Loaded;
            MyMap.Loaded += MyMapControl_Loaded;
        }

        #region Page Events

        private void FreshStart_Loaded(object sender, RoutedEventArgs e)
        {
            mapViewFollow = true;
            goingBackToMainScreen = false;

            //clear tile update
            if (!App.TrackInformationViewModel.Tracer.SessionUnderway)
                TileUpdate.ClearTile();


            if (App.TrackInformationViewModel.KickNewSession)
            {
                if (App.TrackInformationViewModel.LoadRouteId != null)
                {
                    loadedRoute = App.TrackInformationViewModel.Tracer.StoredRoutes.FirstOrDefault(x => x.Id == App.TrackInformationViewModel.LoadRouteId);

                    App.TrackInformationViewModel.LoadRouteId = null;
                }

                if (loadedRoute == null)
                {
                    App.TrackInformationViewModel.Tracer.StartNewSession();
                }
                else
                {
                    App.TrackInformationViewModel.Tracer.LoadSession();
                    DrawPreviousRoute(loadedRoute);
                }

                App.TrackInformationViewModel.KickNewSession = false;
            }
            else
            {
                //draw all existing points
                DrawCurrentRoute(App.TrackInformationViewModel.Tracer.StoredCoordinates);

                CenterMapOnLastPoint();
            }
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (App.TrackInformationViewModel.Tracer.UnexpectedSessionTermination)
            {
                NavigationService.GoBack();
            }

            App.TrackInformationViewModel.PropertyChanged += TrackInformationViewModel_PropertyChanged;

            //draw stored coordinates
            if (App.TrackInformationViewModel.Tracer.CoordinatesGotOffline.Count > 0)
            {
                DrawCurrentRoute(App.TrackInformationViewModel.Tracer.CoordinatesGotOffline, true);

                App.TrackInformationViewModel.Tracer.CoordinatesGotOffline.Clear();

                PerformCenterMap(null, null);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            App.TrackInformationViewModel.PropertyChanged -= TrackInformationViewModel_PropertyChanged;
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (App.TrackInformationViewModel.Tracer.SessionUnderway == true)
            {
                MessageBoxResult returnToMainMenu = MessageBox.Show(AppResources.CurrentlyRecordingMessage, AppResources.ApplicationTitle, MessageBoxButton.OKCancel);

                if (returnToMainMenu == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
                else
                {
                    goingBackToMainScreen = true;

                    App.TrackInformationViewModel.Tracer.StopCurrentSession();
                }
            }
        }

        #endregion

        #region ApplicationBar

        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarMapViewButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/view.png", UriKind.Relative));
            appBarMapViewButton.Text = "map view";
            appBarMapViewButton.Click += MapView_Click;
            ApplicationBar.Buttons.Add(appBarMapViewButton);
        }

        private void MapView_Click(object sender, EventArgs e)
        {
            mapViewFollow = !mapViewFollow;

            PerformCenterMap(null, null);

            UpdateAppBarIconMapView();
        }

        private void TakeScreenshot_Click(object sender, EventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                WriteableBitmap wb = new WriteableBitmap(MyMap, null);
                wb.Render(MyMap, null);
                MemoryStream memoryStream = new MemoryStream();
                wb.SaveJpeg(memoryStream, wb.PixelWidth, wb.PixelHeight, 0, 80);

                string imageName = "WalkCompanionMap_" + DateTime.Now.ToString() + ".jpg";

                MediaLibrary library = new MediaLibrary();
                library.SavePicture(imageName, memoryStream.GetBuffer());

                MessageBox.Show("An image of the current map was successfully store in your pictures library.", "Walk companion", MessageBoxButton.OK);
            });
        }

        private void StoreRoute_Click(object sender, EventArgs e)
        {
            App.TrackInformationViewModel.StoreSession.Execute(null);

            RemoveStoreRouteButton();
        }

        private void UpdateAppBarIconMapView()
        {
            //update ApplicationBarIcon
            ApplicationBarIconButton button = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
            button.IconUri = !mapViewFollow ? new Uri("/Assets/AppBar/view-focused.png", UriKind.Relative) : new Uri("/Assets/AppBar/view.png", UriKind.Relative);
        }

        private void RemoveStoreRouteButton()
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
        }

        #endregion

        #region Map Events

        private void PerformCenterMap(object sender, EventArgs e)
        {
            if (mapViewFollow)
            {
                CenterMapOnLastPoint();
            }
            else
            {
                CenterMapUsingAllPoints();
            }
        }

        private void PerformClearMap(object sender, EventArgs e)
        {
            MyMap.Layers.Clear();
            MyMap.MapElements.Clear();
        }

        private void PerformMapUpdate(object sender, EventArgs e)
        {
            DrawCurrentPoint(App.TrackInformationViewModel.Tracer.CurrentCoordinate);
        }

        // direct map modifiers
        private void CenterMapUsingAllPoints(GeoCoordinate[] coordinates = null)
        {
            if (coordinates == null && (App.TrackInformationViewModel.Tracer.StoredCoordinates.Count > 0 || loadedRoute != null))
            {
                List<GeoCoordinate> pointsToCenter = loadedRoute != null ? loadedRoute.StoredCoordinates : new List<GeoCoordinate>();
                pointsToCenter.AddRange(App.TrackInformationViewModel.Tracer.StoredCoordinates);

                LocationRectangle setRect = LocationRectangle.CreateBoundingRectangle(pointsToCenter.ToArray());

                MyMap.SetView(setRect);
            }
            else if (coordinates != null && coordinates.Length > 0)
            {
                LocationRectangle setRect = LocationRectangle.CreateBoundingRectangle(coordinates);

                MyMap.SetView(setRect);
            }
            else
            {
                mapViewFollow = !mapViewFollow;
            }
        }

        private void CenterMapOnLastPoint()
        {
            if (App.TrackInformationViewModel.Tracer.CurrentCoordinate != null)
            {
                MyMap.SetView(App.TrackInformationViewModel.Tracer.CurrentCoordinate, DEFAULT_ZOOM_LEVEL);
            }
        }

        #endregion

        #region Drawing methods

        private void DrawPreviousRoute(StoredRoute route)
        {
            MapPolyline dynamicPolyline = new MapPolyline();
            dynamicPolyline.StrokeColor = Color.FromArgb(0x70, 0x80, 0x80, 0x80);
            dynamicPolyline.StrokeThickness = 5;
            dynamicPolyline.StrokeDashed = true;

            GeoCoordinateCollection collectionOfPoints = route.ToGeoCoordinateCollection();

            dynamicPolyline.Path = collectionOfPoints;

            DrawPushPinOnLocation(collectionOfPoints.First(), true, true);
            DrawPushPinOnLocation(collectionOfPoints.Last(), false, true);

            MyMap.MapElements.Add(dynamicPolyline);
        }

        private void DrawCurrentRoute(List<GeoCoordinate> currentCoordinates, bool ignoreFirstPoint = false)
        {
            bool firstPoint = !ignoreFirstPoint;

            foreach (GeoCoordinate currentCoordinate in currentCoordinates)
            {
                currentPolyline.Path.Add(currentCoordinate);

                if (firstPoint)
                    DrawPushPinOnLocation(currentCoordinate);

                firstPoint = false;
            }
        }

        private void DrawCurrentPoint(GeoCoordinate currentCoordinate)
        {
            currentPolyline.Path.Add(currentCoordinate);

            if (App.TrackInformationViewModel.Tracer.StoredCoordinates.Count == 1)
                DrawPushPinOnLocation(currentCoordinate);

            DrawnPoints.Add(currentCoordinate);
        }

        private void DrawPushPinOnLocation(GeoCoordinate currentCoordinate, bool isStartingPoint = true, bool isOldRoute = false)
        {
            PushPinControl marker = new PushPinControl();
            string title = isStartingPoint ? AppResources.Start : AppResources.Finish;
            string time = isStartingPoint ? App.TrackInformationViewModel.TimeStarted : App.TrackInformationViewModel.TimeFinished;
            time = !isOldRoute ? time : string.Empty;

            marker.FillDescription(title, time);
            if (!isStartingPoint)
                marker.ChangeBackgroundToRed();

            if (isOldRoute)
                marker.ChangeBackgroundToGrey();

            MapOverlay mylocationOverlay = new MapOverlay();
            mylocationOverlay.Content = marker;
            mylocationOverlay.GeoCoordinate = currentCoordinate;
            MapLayer myLocationLayer = new MapLayer();
            myLocationLayer.Add(mylocationOverlay);
            MyMap.Layers.Add(myLocationLayer);
        }

        #endregion

        private void HandleStoppingSession(object sender, EventArgs e)
        {
            DrawPushPinOnLocation(App.TrackInformationViewModel.Tracer.CurrentCoordinate, false);

            if (goingBackToMainScreen == false)
                ShowFinalScreen();
        }

        private void HandleRouteStored(object sender, EventArgs e)
        {
            MessageBox.Show(AppResources.WalkStoredMessage, AppResources.ApplicationTitle, MessageBoxButton.OK);
        }

        private void ShowFinalScreen()
        {
            if (ApplicationBar.Buttons.Count < 2)
            {
                //add Save button
                ApplicationBarIconButton appBarSaveButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/save.png", UriKind.Relative));
                appBarSaveButton.Text = AppResources.Save;
                appBarSaveButton.Click += StoreRoute_Click;
                appBarSaveButton.IsEnabled = App.TrackInformationViewModel.Tracer.StoredCoordinates.Count > 2;
                ApplicationBar.Buttons.Add(appBarSaveButton);

                //add Image button
                ApplicationBarIconButton appBarImageButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/feature.camera.png", UriKind.Relative));
                appBarImageButton.Text = AppResources.Screenshot;
                appBarImageButton.Click += TakeScreenshot_Click;
                ApplicationBar.Buttons.Add(appBarImageButton);
            }
        }

        private void MyMapControl_Loaded(object sender, RoutedEventArgs e)
        {
            MapsSettings.ApplicationContext.ApplicationId = "50b074db-dfd5-4180-9f1e-385043682f94";
            MapsSettings.ApplicationContext.AuthenticationToken = "KhD43c8YubYT7aJPlBa0Iw";
        }

        private void TrackInformationViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ExecuteUICommand")
            {
                string cmd = App.TrackInformationViewModel.ExecuteUICommand;

                switch (cmd)
                {
                    case "AddPointToMap":
                        PerformMapUpdate(null, null);
                        break;
                    case "CenterMap":
                        PerformCenterMap(null, null);
                        break;
                    case "ClearMap":
                        PerformClearMap(null, null);
                        break;
                    case "StoppinSession":
                        HandleStoppingSession(null, null);
                        break;
                    case "RouteStored":
                        HandleRouteStored(null, null);
                        break;
                }
            }
        }

    }
}