﻿using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Toolkit.Uwp.Notifications;

using MapEngine.Interop.Util;
using WinTak.Display;
using WinTak.Common.Coords;
using WinTak.Common.CoT;
using WinTak.Common.Messaging;
using WinTak.Common.Preferences;
using WinTak.Common.Services;
using WinTak.CursorOnTarget.Services;
using WinTak.Framework;
using WinTak.Framework.Docking;
using WinTak.Framework.Docking.Attributes;
using WinTak.Framework.Messaging;
using WinTak.Framework.Notifications;
using WinTak.Location.Services;

using Hello_World_Sample.Notifications;
using Hello_World_Sample.Common;
using System.ComponentModel;
using System.Collections.Generic;
using WinTak.UI;
using WinTak.CursorOnTarget;
using WinTak.Common.Utils;
using TAKEngine.Core;
using WinTak.Mapping;
using WinTak.Graphics.Map;
using WinTak.Graphics;
using WinTak.Overlays.ViewModels;
using atakmap.cpp_cli.core;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinTak.Overlays.Services;
using Hello_World_Sample.Properties;
using WinTak.Common.Geofence;
using WinTak.Alerts;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using WinTak.Alerts.Notifications;
using Prism.Events;
using WinTak.Display.Controls;
using WinTak.Mapping.Services;
using WinTak.UI.Themes;
using System.Windows;
using WinTak.CursorOnTarget.Placement.DockPanes;
using Windows.ApplicationModel.Contacts;
using WinTak.Net.Contacts;
using WinTak.MissionPackages;
using WinTak.Common.Time;
using WinTak.Common.Location;
using WinTak.Common.Editors;
using WinTak.Location.Views;
using WinTak.Location.Providers;
using System.Threading;

namespace Hello_World_Sample
{
    [DockPane(ID, "HelloWorld", Content = typeof(HelloWorldView), DockLocation = DockLocation.Left, PreferredWidth = 300)]
    /* The DockPane class provides the ability to Hide/Show your dockable window 
     * and serves as the ViewModel for your view.
     * */
    internal class HelloWorldDockPane : DockPane
    {

        internal const string ID = "HelloWorld_HelloWorldDockPane";
        internal const string TAG = "HelloWorldDockPane";
        private readonly ICotMessageSender _cotMessageSender;
        private readonly IContactService _contactList;
        private readonly IImageStore _imageStore;
        private IMissionPackageService _missionPackageService;

        /* Common */
        // public ILogger _logger; // Or use the Log.x like ATAK
        private readonly IMessageHub _messageHub;
        private IUnitDisplayPreferences _unitDisplayPreferences;
        private IStatusIndicator _statusIndicator;
        private ILocationPreferences _locationPreferences;
        private IHelloWorldLocationPreferences _helloWorldLocationPreferences;
        private ILocationProvider _locationProvider;
        public IDevicePreferences _devicePreferences;
        public ILocationService _locationService;

        /* ***** Layout Examples ***** */
        public ICommand LargerBtn { get; private set; }
        public ICommand SmallerBtn { get; private set; }
        public ICommand ShowSearchIconBtn { get; private set; }
        public ICommand RecyclerViewBtn { get; private set; }
        public ICommand TabViewBtn { get; private set; }
        public ICommand OverlayViewBtn { get; private set; }
        public ICommand DropdownBtn { get; private set; }

        public IDockingManager _dockingManager;
        public ICoTManager _coTManager;
        public IElevationManager _elevationManager;
        public IMapGroupManager _mapGroupManager;
        public IMapObjectRenderer _mapObjectRenderer;
        private readonly ImageSource _customMarkerHWImageSource;
        private CompositeMapItem _customMarkerCompositeMapItem;
        private MapObjectItem _customMarker;
        private MapGroup _mapGroup;
        private WheelMenuItem _wheelMenuItem;
        private readonly IPrecisionMoveService _precisionMoveService;
        public DockPaneAttribute DockPaneAttribute { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly IMapViewController _mapViewController;

        public IAlertProvider _alertProvider { get; private set; }
        public IAlert _alert { get; private set; }
        public IGeofenceManager _geofenceManager { get; private set; }
        public IMapObjectItemManager _mapObjectItemManager { get; private set; }

        private string cotGuidGenerate;
        public string callsignName;
        public string inputTextMsg;
        public string CallSignName
        {
            get
            {
                return this.callsignName;
            }
            //set => SetAndSubscribeProperty(ref testInputMesg, value);
            //base.SetProperty(ref testInput)
            set
            {
                if (base.SetProperty(ref callsignName, value, nameof(CallSignName)))
                {
                    Log.d(TAG, "Save/Update the value inside : " + callsignName + "'");
                }
            }
        }
        public string InputTextMsg
        {
            get
            {
                return this.inputTextMsg;
            }
            //set => SetAndSubscribeProperty(ref testInputMesg, value);
            //base.SetProperty(ref testInput)
            set
            {
                if (base.SetProperty(ref inputTextMsg, value, nameof(InputTextMsg)))
                {
                    Log.d(TAG, "Save/Update the value inside : " + inputTextMsg + "'");
                }
            }
        }
        /****** CoT Manager ***** */
        ICotMessageReceiver _cotMessageReceiver;
        /* ***** Map Movement ***** */
        public ICommand FlyBtn { get; private set; }

        /* ***** Marker Manipulation ***** */
        public ICommand SpecialMarkerBtn { get; private set; }
        public ICommand AddAnAircraftBtn { get; private set; }
        public ICommand SvgMarkerBtn { get; private set; }
        public ICommand AddLayerBtn { get; private set; }
        public ICommand AddMultiLayerBtn { get; private set; }
        public ICommand AddHeatMapBtn { get; private set; }
        public ICommand StaleOutMarkerBtn { get; private set; }
        public ICommand AddStreamBtn { get; private set; }
        public ICommand RemoveStreamBtn { get; private set; }
        public ICommand CoordinateEntryBtn { get; private set; }
        public ICommand ItemInspectBtn { get; private set; }
        public ICommand CustomTypeBtn { get; private set; }
        public ICommand CustomMenuFactoryBtn { get; private set; }
        public ICommand ISSLocationBtn { get; private set; }
        public ICommand SensorFOVBtn { get; private set; }

        /* ***** Route Examples ***** */
        public ICommand ListRoutesBtn { get; private set; }
        public ICommand AddXRouteBtn { get; private set; }
        public ICommand ReXRouteBtn { get; private set; }
        public ICommand DropRouteBtn { get; private set; }

        /* ***** Emergency Examples ***** */
        public ICommand EmergencyBtn { get; private set; }
        public ICommand NoEmergencyBtn { get; private set; }

        /* ***** Drawing Examples ***** */
        public ICommand RbcircleBtn { get; private set; }
        public ICommand AddRectangleBtn { get; private set; }
        public ICommand DrawShapesBtn { get; private set; }
        public ICommand GroupAddBtn { get; private set; }
        public ICommand AssociationsBtn { get; private set; }

        /* ***** GPS Examples ***** */
        public ICommand ExternalGpsBtn { get; private set; }

        /* ***** Elevation Examples ***** */
        public ICommand SurfaceAtCenterBtn { get; private set; }

        /* ***** Notification Examples ***** */
        public ICommand GetCurrentNotificationsBtn { get; private set; }
        public ICommand FakeContentProviderBtn { get; private set; }
        public ICommand NotificationSpammerBtn { get; private set; }
        public ICommand NotificationWithOptionsBtn { get; private set; }
        public ICommand NotificationToWinTakToastBtn { get; private set; }
        public ICommand NotificationToWindowsBtn { get; private set; }
        public ICommand VideoLauncherBtn { get; private set; }
        public ICommand AddToolbarItemBtn { get; private set; }
        public ICommand AddCountToIconBtn { get; private set; }

        public INotificationLog _notificationLog;

        /* ***** Images and Camera */
        public ICommand CameraLauncherBtn { get; private set; }
        public ICommand ImageAttachBtn { get; private set; }
        public ICommand WebViewBtn { get; private set; }
        public ICommand MapScreenshotBtn { get; private set; }

        /* ***** Speach To Text ***** */
        public ICommand SpeechToTextBtn { get; private set; }
        public ICommand SpeechToActivityBtn { get; private set; }
        /* ***** Sensors ***** */
        public ICommand BumpControlBtn { get; private set; }
        /* ***** Navigation ***** */
        public ICommand HookNavigationEventsNameBtn { get; private set; }
        /* ***** Lower Level Examples ***** */
        public ICommand GetImagesBtn { get; private set; }
        /* ***** Map Layers ***** */
        public ICommand DownloadMapLayerBtn { get; private set; }
        /* ***** Spinner Examples ***** */
        public ICommand Spinner1Btn { get; private set; }

        /* ***** Plugin Template Duplicate (From WinTAK-Documentation) ***** */
        public ICommand IncreaseCounterBtn { get; private set; }
        public ICommand WhiteHouseCoTBtn { get; private set; }
        private int _counter;
        private double _mapFunctionLat;
        private double _mapFunctionLon;
        private bool _mapFunctionIsActivate;

        // -- ----- ----- ----- ----- CONSTRUCTOR ----- ----- ----- ----- -- //
        [ImportingConstructor] // this import provide the capability to get WinTAK exposed Interfaces
        public HelloWorldDockPane(
            ICommunicationService communicationService,
            ICoTManager coTManager,
            ICotMessageReceiver cotMessageReceiver,
            ICotMessageSender cotMessageSender,
            IDevicePreferences devicePreferences,
            IDockingManager dockingManager,
            IElevationManager elevationManager,
            IGeocoderService geocoderService,
            ILogger logger,
            ILocationService locationService,
            /* IMapObjectFinderService mapObjectFinderService, */
            IMapGroupManager mapGroupManager,
            IMapObjectItemManager mapObjectItemManager,
            IMapObjectRenderer mapObjectRenderer,
            IMessageHub messageHub,
            INotificationLog notificationLog,
            IMapViewController mapViewController,

            // test the video
            //IMediaElement mediaElement,
            //IVideoOverlay videoOverlay,
            //IVideoOverlayProvider videoOverlayProvider
            Gv2FPlayer.IVideoPane videoPane,
            Gv2FPlayer.IVideoControlsExtender videoControlsExtender,
            Gv2FPlayer.IVideoControlsExtension videoControlsExtension,

            IGeofenceManager geofenceManager,

            IPrecisionMoveService precisionMoveMarking,

            IContactService contactService,
            IImageStore imageStore,
            IMissionPackageService missionPackageService,
            IUnitDisplayPreferences unitDisplayPreferences,
            IStatusIndicator statusIndicator,
            ILocationPreferences locationPreferences
            //IHelloWorldLocationPreferences helloWorldLocationPreferences
            )
        {

            // test video
            //try { Log.d(TAG, "MediaElement : " + mediaElement); }
            //catch (Exception ex) { Log.e(TAG, "MediaElement : " + ex.ToString()); }
            //try { Log.d(TAG, "VideoOverlay : " + videoOverlay); }
            //catch (Exception ex) { Log.e(TAG, "VideoOverlay : " + ex.ToString()); }
            //try { Log.d(TAG, "VideoOverlayProvider : " + videoOverlayProvider); }
            //catch (Exception ex) { Log.e(TAG, "VideoOverlayProvider : " + ex.ToString()); }

            //WinTak.Video.IMediaElement mediaElement = null;
            //WinTak.Video.IVideoOverlay videoOverlay = null;
            //WinTak.Video.IVideoOverlayProvider videoOverlayProvider = null;
            //WinTak.Video.MediaFrameEventArgs mediaFrameEventArgs = null;
            //WinTak.Video.MetaDataEventArgs metaDataEventArgs = null;
            Log.d(TAG, MethodBase.GetCurrentMethod() + " - " + videoPane.ToString());
            Log.d(TAG, MethodBase.GetCurrentMethod() + " - " + videoControlsExtender.ToString());
            Log.d(TAG, MethodBase.GetCurrentMethod() + " - " + videoControlsExtension.ToString());
            Log.d(TAG, "We successfully read the videoPane");
            WinTak.Video.IMediaElement mediaElement = videoPane.MediaElement;

            Gv2FPlayer.Views.SelectVideoAliasView selectVideoAliasView;
            Gv2FPlayer.ConnectionEntry connectionEntry = new Gv2FPlayer.ConnectionEntry();
            connectionEntry.Alias = "test";
            connectionEntry.Address = "http://";
            connectionEntry.RtspReliable = true;
            connectionEntry.Active = true;

            // Test
            _cotMessageSender = cotMessageSender;
            _contactList = contactService;
            _imageStore = imageStore;
            _missionPackageService = missionPackageService;
            _communicationService = communicationService;
            /* Interface link */
            //_logger = logger;
            _messageHub = messageHub;
            _dockingManager = dockingManager;
            _locationService = locationService;
            _locationService.PositionChanged += OnPositionChanged;

            Log.i(TAG, "_locationService.GetGpsHeading()       : " + _locationService.GetGpsHeading());
            Log.i(TAG, "_locationService.GetGpsMarker()        : " + _locationService.GetGpsMarker());
            Log.i(TAG, "_locationService.GetGpsObject()        : " + _locationService.GetGpsObject());
            Log.i(TAG, "_locationService.GetGpsPosition()      : " + _locationService.GetGpsPosition());
            Log.i(TAG, "_locationService.GetGpsSpeed()         : " + _locationService.GetGpsSpeed());
            Log.i(TAG, "_locationService.GetHashCode()         : " + _locationService.GetHashCode());
            Log.i(TAG, "_locationService.GetPositionDocument() : " + _locationService.GetPositionDocument());
            Log.i(TAG, "_locationService.GetSelfCotEvent()     : " + _locationService.GetSelfCotEvent());
            Log.i(TAG, "_locationService.GetType()             : " + _locationService.GetType());
            //Log.i(TAG, "_locationService.ConnectionStatus      : " + _locationService.ConnectionStatus);
            //Log.i(TAG, "_locationService.HasConnections        : " + _locationService.HasConnections);
            //Log.i(TAG, "_locationService.IsSimulatedGps        : " + _locationService.IsSimulatedGps);

            _unitDisplayPreferences = unitDisplayPreferences;
            _statusIndicator = statusIndicator;
            _locationPreferences = locationPreferences;
            //_locationProvider = locationProvider;
            //_helloWorldLocationPreferences = helloWorldLocationPreferences;

            _devicePreferences = devicePreferences;
            _notificationLog = notificationLog;
            _coTManager = coTManager;
            _elevationManager = elevationManager;
            _mapGroupManager = mapGroupManager;
            _mapObjectRenderer = mapObjectRenderer;
            _precisionMoveService = precisionMoveMarking;
            _customMarkerHWImageSource = new BitmapImage(new Uri("pack://application:,,,/Hello World Sample;component/assets/brand_cthulhu.png"));
            _customMarkerCompositeMapItem = new CompositeMapItem();
            Log.d(TAG, "The customMarkerCompositeMapItem : " + this._customMarkerCompositeMapItem.GetUid());
            
            _mapObjectItemManager = mapObjectItemManager;
            ICollection<MapObjectItem> rootItems = mapObjectItemManager.RootItems;
            if (rootItems != null)
            {
                _customMarker = new LegacyMapObjectItem(Resources.btnRecyclerViewName, new BitmapImage(new Uri("pack://application:,,,/Hello World Sample;component/assets/brand_cthulhu.png")))
                {
                    Visible = true,
                    Selectable = false,
                    Id = "HelloWorld CustomMarker"
                };
                rootItems.Add(_customMarker);
            }
            _mapGroup = mapGroupManager.GetOrCreateMapGroup("HelloWorldMapGroup");
            _customMarkerCompositeMapItem.Disposing += CustomHWOnDisposing;

            this.CallSignName = _devicePreferences.Callsign;
            this.InputTextMsg = "A default text message from constructor.";
            Log.d(TAG, "" + locationService.GetGpsPosition()); 
            foreach(MapObjectItem mapObjectItem in rootItems)
            {
                if (mapObjectItem.Text == "Geo Fences")
                {
                    Log.d(TAG, "MapObjectItem : " + mapObjectItem.ToString());
                    Log.d(TAG, "Text : " + mapObjectItem.Text);
                    Log.d(TAG, "We have the Geo Fences Overlay");
                    int itemCount = mapObjectItem.GetSubItemCount(); // put 0 becasue does not point to the Geo Fences icons but to the sub items.
                                                                     // We nee to list the items inside of it/
                    Log.d(TAG, "Number of items : " + itemCount.ToString());
                    Log.d(TAG, "Id : " + mapObjectItem.Id);
                    int childCount = mapObjectItem.ChildCount;
                    Log.d(TAG, "ChildCount : " + childCount.ToString());
                    //MapItem mapItems = mapObjectItem.MapItem;
                    //Log.d(TAG, "mapItems : " + mapItems.ToString());

                    ObservableCollection<MapObjectItem> childMapObjectItems = mapObjectItem.Children;
                    foreach (MapObjectItem moi in childMapObjectItems)
                    {
                        Log.d(TAG, "Text : " + moi.Text);
                        Log.d(TAG, "Show Settings : " + moi.ShowSettings);
                        Log.d(TAG, "Id : " + moi.Id);
                        Log.d(TAG, "InViewChildCount : " + moi.InViewChildCount);
                        Log.d(TAG, "Location : " + moi.Location);
                        Log.d(TAG, "Properties : " + moi.Properties);
                        Log.d(TAG, "Position : " + moi.Position);
                        Log.d(TAG, "ShowDetailsCommand : " + moi.ShowDetailsCommand);
                        Log.d(TAG, "SubText : " + moi.SubText);
                        Log.d(TAG, "ToolTip : " + moi.ToolTip);
                        Log.d(TAG, "MapItem : " + moi.MapItem);
                        Log.d(TAG, "ChildCount : " + moi.ChildCount);
                        Log.d(TAG, "Children : " + moi.Children);
                        //Log.d(TAG, " : " + moi.PropertyChanged());
                    }

                }
            }
            foreach (var item in _mapGroupManager.MapItems)
            {
                Log.d(TAG, "MapGroup : " + item.ToString());
                Log.d(TAG, "Text : " + item.Uid);
                Log.d(TAG, "MapItems : " + item.MapItems);
                Log.d(TAG, "ParentMapGroup : " + item.ParentMapGroup);
                Log.d(TAG, "GetCallsign : " + item.GetCallsign());
                Log.d(TAG, "GetUid : " + item.GetUid());
                Log.d(TAG, "Name : " + item.Name);
            }
            _mapViewController = mapViewController;
            this._mapViewController.WheelMenuOpening += MapViewController_WheelMenuOpening;
            // Layout Examples
            LayoutExamples_Configuration();

            // Marker Manipulation - Special Marker
            var specialMarkerCommand = new ExecutedCommand();
            specialMarkerCommand.Executed += OnDemandExecuted_SpecialMarkerButton;
            SpecialMarkerBtn = specialMarkerCommand;

            // Marker Manipulation - Add Streams
            _cotMessageReceiver = cotMessageReceiver;
            var addStreamCommand = new ExecutedCommand();
            addStreamCommand.Executed += OnDemandExecuted_AddStreamBtn;
            AddStreamBtn = addStreamCommand;

            // Notification Examples
            NotificationExamples_Configuration();

            // Plugin Template Duplicate (From WinTAK-Documentation)
            var counterButtonCommand = new ExecutedCommand();
            counterButtonCommand.Executed += OnDemandExecuted_IncreaseCounterBtn;
            IncreaseCounterBtn = counterButtonCommand;

            // Plugin Template Duplicate (From WinTAK Reference Documentation)
            var whiteHouseCoTCommand = new ExecutedCommand();
            whiteHouseCoTCommand.Executed += OnDemandExecuted_WhiteHouseCoTBtn;
            WhiteHouseCoTBtn = whiteHouseCoTCommand;


            // Test Geofence - moved to a services
            //_geofenceManager = geofenceManager;
            //geofenceManager.GetGeofences();
            //foreach(GeofenceData geofenceData in geofenceManager.GetGeofences())
            //{
            //    // Get the information of all geofence in the wintak instance.
            //    Log.d(TAG, "MapItemUid : " + geofenceData.MapItemUid.ToString());
            //    Log.d(TAG, "MonitorType : " + geofenceData.MonitoredType.ToString());
            //    Log.d(TAG, "Trigger : " + geofenceData.Trigger.ToString());
            //    MonitoredTypes monitored = geofenceData.MonitoredType;
            //    Trigger trigger = geofenceData.Trigger;
            //    //public enum MonitoredTypes
            //    //{
            //    //    TAKUsers,
            //    //    Friendly,
            //    //    Hostile,
            //    //    Custom,
            //    //    All
            //    //}
            //    //public enum Trigger
            //    //{
            //    //    Entry,
            //    //    Exit,
            //    //    Both
            //    //}

            //    //GeofenceAlertMessage geofenceAlertMessage = new GeofenceAlertMessage(geofenceData.MapItemUid, );
            //}
            //string geofenceBreached =  WinTak.Common.Properties.Resource_Civilian.GeofenceBreached;
            
            //geofenceManager.GeofenceChanged += GeofenceManager_GeofenceChanged;
            //Log.d(TAG, "Geofence Breached test ?" + geofenceBreached); // seens that is only a string and not something that we can manage
            //GeofenceAlertMessage geofenceAlertMessage = new GeofenceAlertMessage();

            // How to monitor them when something entering into it ?
            //IEnumerable<IAlert> alerts = _alertProvider.Alerts; // Currently alertProvier.Alerts return a null
            //foreach (IAlert alert in alerts)
            //{
                //Log.d(TAG, "Alert : " + alert.ToString());
            //}
        }

        //private void OnPositionChanged(object sender, WinTak.Common.Location.PositionChangedEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        // This method is raised only when a geofence is modified. In any case, this method is raised when the geofence is breached.
        private void GeofenceManager_GeofenceChanged(object sender, GeofenceData e)
        {
            Log.d(TAG, MethodBase.GetCurrentMethod() + " - " + e.ToString());
        }
        // --------------------------------------------------------------------
        // Common Method
        // --------------------------------------------------------------------
        private class ExecutedCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;
            public event EventHandler Executed;

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                Executed?.Invoke(this, EventArgs.Empty);
            }

        }

        private void GetMEFActiveInterface()
        {
            //Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .ToArray();
            Log.d(TAG, "Under test to check information");
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    var exportedInterfaces = assembly.GetExportedTypes().Where(type => type.IsInterface && type.IsDefined(typeof(InheritedExportAttribute)));
                    var importedInterfaces = assembly.GetExportedTypes().SelectMany(type => type.GetProperties().Where(prop => prop.PropertyType.IsInterface && prop.IsDefined(typeof(ImportAttribute)))).Select(prop => prop.PropertyType).Distinct();

                    foreach (var exportedInterface in exportedInterfaces)
                    {
                        Log.d(TAG, MethodBase.GetCurrentMethod() + " Exported Interface : " + exportedInterface.FullName);
                    }

                    foreach (var importedInterface in importedInterfaces)
                    {
                        Log.d(TAG, MethodBase.GetCurrentMethod() + " Imported Interface : " + importedInterface.FullName);
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    foreach (Exception innerEx in ex.LoaderExceptions)
                    {
                        Log.e(TAG, MethodBase.GetCurrentMethod() + " Error loading assembly: {innerEx.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Log.e(TAG, MethodBase.GetCurrentMethod() + ex.ToString());
                }
            }
        }

        private string ImageToBase64(System.Drawing.Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to base64 string
                string base64String = Convert.ToBase64String(imageBytes);

                return $"data:image/png;base64,{base64String}";
            }
        }

        private Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.DrawImage(image, 0, 0, width, height);
            }
            return resizedImage;
        }

        private string SaveImageToFile(Bitmap image, string imgName)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), imgName);
            image.Save(tempFilePath, System.Drawing.Imaging.ImageFormat.Png);
            return tempFilePath;
        }
        
        private void SetAndSubscribeProperty<T>(ref T backingField, T newValue) where T : INotifyPropertyChanged
        {
            Log.d(TAG, MethodBase.GetCurrentMethod() + " - " + backingField + " - " + newValue);
            if (!EqualityComparer<T>.Default.Equals(backingField, newValue))
            {
                backingField.PropertyChanged -= HandlePropertyChanged;
            }
            else
            {
                backingField.PropertyChanged += HandlePropertyChanged;
            }
            base.SetProperty(ref backingField, newValue);
        }
        
        protected void OnPropertyChanged(string propertyname)
        {
            Log.d(TAG, MethodBase.GetCurrentMethod() + propertyname);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CallSignName))
            {
                OnPropertyChanged(e.PropertyName); // Notify property change
            }
        }

        
        // --------------------------------------------------------------------
        // Layout Examples
        // --------------------------------------------------------------------

        private void LayoutExamples_Configuration()
        {
            // Layout Example - Larger
            var largerCommand = new ExecutedCommand();
            largerCommand.Executed += OnDemandExecuted_LargerButton;
            LargerBtn = largerCommand;

            // Layout Example - Smaller
            var smallerCommand = new ExecutedCommand();
            smallerCommand.Executed += OnDemandExecuted_SmallerButton;
            SmallerBtn = smallerCommand;

            // Layout Example - Show Search Icon
            var showSearchIconCommand = new ExecutedCommand();
            showSearchIconCommand.Executed += OnDemandExecuted_ShowSearchIcon;
            ShowSearchIconBtn = showSearchIconCommand;

            // Layer Example - Recycler View
            var recyclerViewCommand = new ExecutedCommand();
            recyclerViewCommand.Executed += OnDemandExecuted_RecyclerViewBtn;
            RecyclerViewBtn = recyclerViewCommand;
        }
        /* Layout Example - Larger Button
         * --------------------------------------------------------------------
         * Desc. : Larger button is to Float the Dockpane. 
         * */
        private void OnDemandExecuted_LargerButton(object sender, EventArgs e)
        {
            Log.d(TAG, MethodBase.GetCurrentMethod() + "");
            Log.d(TAG, MethodBase.GetCurrentMethod() +" : " +_locationService.GetGpsPosition());
            Log.d(TAG, MethodBase.GetCurrentMethod() + " : " + _locationService);
        }

        /* Layout Example - Smaller Button
         * --------------------------------------------------------------------
         * Desc. :
         * */
        CotDockPane dockPane;
        private ICommunicationService _communicationService;

        private void OnDemandExecuted_SmallerButton(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
            TAKEngine.Core.GeoPoint test = new TAKEngine.Core.GeoPoint(44.238366, 9.6912326, 200.01, TAKEngine.Core.AltitudeReference.HAE);

            dockPane = new CotDockPane(_cotMessageSender, _cotMessageReceiver,
                _contactList, _messageHub, _imageStore, _missionPackageService,
                _locationService, _communicationService, _dockingManager);
            Log.d(TAG, "" + dockPane);
            Log.d(TAG, "selfPosition : before : " + dockPane.SelfPosition);

            dockPane.SelfPosition = test;
            Log.d(TAG, "selfPosition : after : " + dockPane.SelfPosition);
            CotEvent cotEvent = _locationService.GetSelfCotEvent();
            Log.d(TAG, "Last Position with CotEvent : " + cotEvent);
            Log.d(TAG, "" + cotEvent.GetSchema());
            CotPoint cotPoint = new CotPoint(test);
            cotEvent.Time = CoordinatedTime.FromCot(CoordinatedTime.CurrentDate().ToString());
            cotEvent.Start = CoordinatedTime.FromCot(CoordinatedTime.CurrentDate().ToString());

            //CotDetail cotDetail = new CotDetail();
            Log.d(TAG, "cotEvent.Detail : " + cotEvent.Detail);
            CotDetail cotDetail = cotEvent.Detail;


            cotEvent.Point = cotPoint;
            if (cotEvent.IsValid())
            {
                // dockPane.Send(cotEvent); // send is calling the send function.
                // Opening the dockpane send not updating / sending the cot itself
                Log.d(TAG, "New Position with CotEvent : " + cotEvent);
            }
            else
            {
                Log.e(TAG, "CotEvent is not valid !!! Here is the value of it : " + cotEvent);
            }
            // // Marker Manipulation - Special Marker
            double testSpeed = 0.0;
            double testHeading = 0.0;

            PositionChangedEventArgs positionArgs = new PositionChangedEventArgs(test, testSpeed, testHeading);
            //OnPositionChanged(this, positionArgs);
            MapItem mapItem = _locationService.GetGpsObject();
            MapMarker mapMarker = _locationService.GetGpsMarker();
            mapItem.SetMarkerPosition(test);
            Log.d(TAG, "mapMarker : " + mapMarker);
            Log.d(TAG, "mapItem : " + mapItem);

            // we create a new and not get the one which is currently existing
            LocationPanel locationPanel = new LocationPanel(_messageHub, _unitDisplayPreferences);
            locationPanel.ManualPositionRequested += LocationPanel_ManualPositionRequested;
            LocationPanel_ManualPositionRequested(this, EventArgs.Empty);

            Log.d(TAG, "Test LocationPanel.Callsign : " + locationPanel.Callsign);
            Log.d(TAG, "Test LocationPanel.Position : " + locationPanel.Position);

            //_locationService.StartSimulatedGps();

            ManualResetEvent manualResetEvent;
            ConnectionStatus ConnectionStatus = ConnectionStatus.Connecting;
            ThreadPool.QueueUserWorkItem(delegate (object o)
            {
                TAKEngine.Core.GeoPoint position = (TAKEngine.Core.GeoPoint)o;
                mapItem.SetMarkerPosition(position);
                //ILocationProvider iprovider = new global::atakmap.i(mapItem);
                //ManualResetEvent.WaitOne();

            }, new TAKEngine.Core.GeoPoint(test));

            WinTak.Location.Providers.PositionUpdatedEventArgs positionUpdatedEventArgs = new WinTak.Location.Providers.PositionUpdatedEventArgs(test, (FixQuality)testSpeed, testHeading);



            //if (System.Windows.Application.Current.Dispatcher.CheckAccess())
            //{
            //    if (_locationProvider != null)
            //    {
            //        _locationProvider.StartAsync();
            //    }
            //    else
            //    {
            //        System.Windowa.Application.Current.Dispatcher.Invoke(() =>
            //        {
            //            if (_locationProvider != null)
            //            {
            //                _locationProvider.StartAsync();
            //            }
            //        });
            //    }
            //}

            _statusIndicator.SetSelfMarkerRequested += StatusIndicator_SetSelfMarkerRequest;

            //mapMarker = new MapMarker();
            //mapMarker.ClampToSurface = true;
            //mapMarker.Visible = true;
            //mapMarker.Position = test;
            //mapMarker.Orientation = MapMarkerBase.OrientationMode.Absolute;

            _locationPreferences.GpsTypeChanged += LocationPreferences_GpsTypeChanged;
            Log.d(TAG, "FollowTargetUpdateSpeed : " + _locationPreferences.FollowTargetUpdateSpeed);
            Log.d(TAG, "GpsProvider : " + _locationPreferences.GpsProvider);
            Log.d(TAG, "GpsType : " + _locationPreferences.GpsType);
            Log.d(TAG, "BaudRate : " + _locationPreferences.BaudRate);
            Log.d(TAG, "ListenPort : " + _locationPreferences.ListenPort);
            Log.d(TAG, "UseGpsTime : " + _locationPreferences.UseGpsTime);
            Log.d(TAG, "RequestTimeout : " + _locationPreferences.RequestTimeout);
            Log.d(TAG, "StationaryLocation : " + _locationPreferences.StationaryLocation);
            
        }

        private void LocationPreferences_GpsTypeChanged(object sender, GpsType e)
        {
            Log.d(TAG, MethodBase.GetCurrentMethod() + " : " + e);
        }
        private void StatusIndicator_SetSelfMarkerRequest(object sender, EventArgs e)
        {
            Log.d(TAG, "StatusIndicator_SetSelfMarkerRequest : " + sender);
            Log.d(TAG, "StatusIndicator_SetSelfMarkerRequest : " + e);

        }
        private void LocationPanel_ManualPositionRequested(object sender, EventArgs e)
        {
            Log.d(TAG, MethodBase.GetCurrentMethod() + "");
        }
        private void OnPositionChanged(object sender, PositionChangedEventArgs e)
        {
            var newPosition = e.Position;
            var newSpeed = e.Speed;
            var newHeading = e.Heading;
            Log.d(TAG, "OnPositionChanged : newPosition : " + newPosition);
            UpdateSelfPosition(newPosition);
        }

        private void UpdateSelfPosition(TAKEngine.Core.GeoPoint newPosition)
        {
            Log.d(TAG, "UpdateSelfPosition ?");
            
        }

        private void MoveMarker(IEditable marker, TAKEngine.Core.GeoPoint point, bool keepAltitude, PreciseCoordDetails precise)
        {
            if (!(marker.Geometry is IPoint point2))
            {
                Log.e(TAG, "Marker is not a Ipoint");
                return;
            }
            _ = point2.Location;
            TAKEngine.Core.GeoPoint geoPoint = new TAKEngine.Core.GeoPoint(point);
            if (keepAltitude)
            {
                geoPoint.Altitude = this._elevationManager.GetElevation(geoPoint);
                geoPoint.AltitudeRef = global::TAKEngine.Core.AltitudeReference.HAE;

            }
            point2.Location = geoPoint;
            if (marker.Subject != null)
            {
                if (precise != null)
                {

                } // else if ()
                //{

                //}
            }
        }

        /* Layout Example - Show Search Icon
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_ShowSearchIcon(object sender, EventArgs e)
        {
            /* ATAK implementation : 
             * // The button bellow shows how one might go about
             * // setting up a custom map widget.
             * final Button showSearchIcon = helloView.findViewById(R.id.showSearchIcon);
             * showSearchIcon.setOnClickListener(new OnClickListener() {
             *  @Override
             *  public void onClick(View v) {
             *      Log.d(TAG, "sending broadcast SHOW_MY_WACKY_SEARCH");
             *      Intent intent = new Intent("SHOW_MY_WACKY_SEARCH");
             *      AtakBroadcast.getInstance().sendBroadcast(intent);
             *  }
             * }); // the image shown when we click on it is the sync_search.png -> AndroidTacticalAssaultKit-CIV-master\atak\ATAK\app\src\main\res\drawable-hdpi
             */

            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
            
            Prompt.Show("Place the marker on the map.");
            _mapGroupManager.ItemAdded += MapObjectAdded;
            MapViewControl.PushMapEvents(MapMouseEvents.MapMouseDown
                                        | MapMouseEvents.MapMouseMove
                                        | MapMouseEvents.MapMouseUp
                                        | MapMouseEvents.ItemDrag
                                        | MapMouseEvents.ItemDragCompleted
                                        | MapMouseEvents.ItemLongPress
                                        | MapMouseEvents.MapDrag
                                        | MapMouseEvents.MapLongPress
                                        | MapMouseEvents.MapDoubleClick
                                        | MapMouseEvents.ItemDoubleClick);
            MapViewControl.MapClick += PlaceHelloWorld_MapClick;

        }
        private void PlaceHelloWorld_MapClick(object sender, MapMouseEventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
            
            /* Variables declaration */
            string cotUid;
            string cotType = "a-f-A";
            string cotName = "HWM"; // HelloWorldMarker
            string cotDetail;
            
            /* Implementation */
            cotUid = Guid.NewGuid().ToString();
            cotName = _coTManager.CreateCallsign(cotName, CallsignCreationMethod.BasedOnTypeAndDate);
            cotDetail = "<archive /> <_helloworld_ title=\"" + cotName + "\" /><precisionlocation altsrc=\"DTED0\" />"; ;
            TAKEngine.Core.GeoPoint geoPoint;
            geoPoint = new TAKEngine.Core.GeoPoint(e.WorldLocation)
            {
                Altitude = _elevationManager.GetElevation(e.WorldLocation),
                AltitudeRef = global::TAKEngine.Core.AltitudeReference.HAE
            };
            
            if (double.IsNaN(geoPoint.Altitude))
            {
                geoPoint.Altitude = Altitude.UNKNOWN_VALUE;
            }

            cotGuidGenerate = cotUid;
            _coTManager.AddItem(cotUid, cotType, geoPoint, cotName, cotDetail);
            
            MapViewControl.PopMapEvents();
            Prompt.Clear();
        }
        private void MapObjectAdded(object sender, MapItemEventArgs args)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
            MapItem mapItem;
            mapItem = args?.MapItem;
            if (mapItem == null || cotGuidGenerate == null || cotGuidGenerate == mapItem.GetUid())
            {
                return;
            }
            base.DispatchAsync(delegate
            {
                if (_dockingManager.GetDockPane(ID) is HelloWorldDockPane helloWorldDockPane)
                {
                    MapMarker mapMarker;
                    mapMarker = mapItem.GetMapMarker();
                    if (mapMarker != null)
                    {
                        // helloWorldDockPane.SetMarker(mapMarker);
                        mapItem.Properties.TryGetValue("helloworldMapItem", out var value);
                        if (value != null)
                        {
                            ((MapObjectItem)value).Text = mapItem.Properties["akey?"].ToString();
                        }
                        cotGuidGenerate = null;
                    }
                }
            });
        }
        public void MapViewController_WheelMenuOpening(object sender, MenuPopupEventArgs e)
        {
            Log.d(TAG, "MapViewController_WheelMenuOpening() - Starting");

            // Log the type of 'sender'
            Log.d(TAG, "Sender Type: " + (sender?.GetType().ToString() ?? "null"));

            // Check if sender is WheelMenu
            if (sender is WheelMenu wheelMenu)
            {
                Log.d(TAG, "Sender is WheelMenu");
               

                // Attempt to get the clickedParentObject
                if (e.GetSingleClickedItem(out var clickedParentObject) != null)
                {
                    Log.d(TAG, "Clicked parent object is: " + clickedParentObject?.GetType().ToString());

                    // Check if clickedParentObject is MapItem
                    if (clickedParentObject is MapItem mapItem)
                    {
                        Log.d(TAG, "Clicked object is MapItem with Type : " + mapItem.GetType() + " _customMarkerCompositeMapItem : " + this._customMarkerCompositeMapItem.GetType());
                        Log.d(TAG, "Clicked object is MapItem with Name: " + mapItem.Name + " _customMarkerCompositeMapItem: " + this._customMarkerCompositeMapItem.Name);
                        Log.d(TAG, "Clicked object is MapItem with Text: " + mapItem.Properties + " _customMarkerCompositeMapItem: " + this._customMarkerCompositeMapItem.Properties);
                        Log.d(TAG, "Clicked object is MapItem with UID: " + mapItem.GetUid() + " _customMarkerCompositeMapItem: " + this._customMarkerCompositeMapItem.GetUid());

                        IDictionary<string, object> dictionaries = _customMarkerCompositeMapItem.Properties;

                        foreach(var dictionary in dictionaries)
                        {
                            Log.d(TAG, $"_customMarkerCompositeMapItem : Key: {dictionary.Key}, Value: {dictionary.Value}");
                        }

                        IDictionary<string, object> dictionaries2 = mapItem.Properties;
                        foreach (var dictionary in dictionaries2)
                        {
                            Log.d(TAG, $"mapItem : Key: {dictionary.Key}, Value: {dictionary.Value}");
                        }
                        //if (dictionary != null)
                        //{
                        //    // Iterate through all key-value pairs
                        //    foreach (var keyValuePair in dictionary)
                        //    {
                        //        Log.d(TAG, $"Key: {keyValuePair.Key}, Value: {keyValuePair.Value}");
                        //    }

                        //    // To access a specific key's value, for example

                        //}

                        // Check if the UIDs match
                        if (mapItem.GetUid().Equals(this._customMarkerCompositeMapItem.GetUid()))
                        {
                            Log.d(TAG, "MapItem UIDs match. Passed the first if statement.");

                            // Proceed with the rest of the logic
                            StandardActions.AddDelete(mapItem, wheelMenu, disabled: false);
                            MapMarker mapMarker = mapItem.GetMapMarker();

                            // Log if MapMarker is not null
                            if (mapMarker != null)
                            {
                                Log.d(TAG, "MapMarker is not null");
                                this._precisionMoveService.AddPrecisionMove(mapMarker, wheelMenu);
                            }
                            else
                            {
                                Log.d(TAG, "MapMarker is null");
                            }

                            // Remove the delete menu item
                            wheelMenu.Items.Remove(wheelMenu.Items.FirstOrDefault((WheelMenuItem x) => x.Id == "delete"));
                        }
                        else if (dictionaries2.TryGetValue("type", out var value))
                        {
                            if (value is string stringValue)
                            {
                                if (stringValue == "a-f-A")
                                {
                                    Log.d(TAG, "MapItem Type. Passed the else if statement.");

                                    // Proceed with the rest of the logic
                                    //StandardActions.AddDelete(mapItem, wheelMenu, disabled: false); // Create an empty space ? which shown on the figure previously
                                    MapMarker mapMarker = mapItem.GetMapMarker();

                                    WheelMenuItem _detailsWheelItem = new WheelMenuItem("brand_cthulhu", Application.Current.Resources[Icons.BreadcrumbActiveRadialMenuIconKey] as ImageSource, OnDetailsWheelItemClick)
                                    {
                                        Id = WinTak.Common.Properties.Resources.AddGeofence,
                                        ToolTip = "Test Sub Items Menu",
                                    };

                                    // Log if MapMarker is not null
                                    if (mapMarker != null)
                                    {
                                        Log.d(TAG, "MapMarker is not null");
                                        //this._precisionMoveService.AddPrecisionMove(mapMarker, wheelMenu); // Create an empty space ? which shown on the figure previously
                                    }
                                    else
                                    {
                                        Log.d(TAG, "MapMarker is null");
                                    }

                                    // Remove the delete menu item
                                    //wheelMenu.Items.Remove(wheelMenu.Items.FirstOrDefault((WheelMenuItem x) => x.Id == "delete"));
                                    wheelMenu.Items.Add(_detailsWheelItem);
                                }
                            }
                        }
                        else
                        {
                            Log.d(TAG, "MapItem UIDs do not match");
                        }
                    }
                    else
                    {
                        Log.d(TAG, "Clicked parent object is not MapItem");
                    }
                }
                else
                {
                    Log.d(TAG, "GetSingleClickedItem returned null or no clicked parent object");
                }
            }
            else
            {
                Log.d(TAG, "Sender is not WheelMenu");
            }

            Log.d(TAG, "MapViewController_WheelMenuOpening() - End");
            //if (sender is WheelMenu wheelMenu && e.GetSingleClickedItem(out var clickedParentObject) != null && clickedParentObject is MapItem mapItem && mapItem.GetUid().Equals(this._customMarkerCompositeMapItem.GetUid()))
            //{
            //    StandardActions.AddDelete(mapItem, wheelMenu, disabled: false);
            //    MapMarker mapMarker = mapItem.GetMapMarker();
            //    Log.d(TAG, "MapViewController_WheelMenuOpening() - First If passed");
            //    if (mapMarker != null)
            //    {
            //        this._precisionMoveService.AddPrecisionMove(mapMarker, wheelMenu);
            //        Log.d(TAG, "MapViewController_WheelMenuOpening() - Second If passed");
            //    }
            //    wheelMenu.Items.Remove(wheelMenu.Items.FirstOrDefault((WheelMenuItem x) => x.Id == "delete"));
            //}
            //Log.d(TAG, "MapViewController_WheelMenuOpening() - End");
        }

        private void OnDetailsWheelItemClick(object sender, EventArgs e)
        {
            MapEngine.Interop.Util.Log.d(TAG, "OnDetailsWheelItemClick() start");

            Log.d(TAG, "Sender Type: " + (sender?.GetType().ToString() ?? "null"));

            // Do something when the WheelMenuItem is clicked
            object tag = ((WheelMenuItemBase)(WheelMenuItem)sender).Tag;
            MapItem val = (MapItem)((tag is MapItem) ? tag : null);
            if (((WheelMenuItem)sender).Tag is MapItem mapItem && mapItem.Properties.ContainsKey("uid"))
            {
                Guid id = new Guid(mapItem.Properties["uid"].ToString());
                //ShowDockPane(id);
            }
        }

        //public void SetMarker(MapMarker marker) { }

        /* Layout Example - Recycler View
            * --------------------------------------------------------------------
            * Desc. :
            * */
        private void OnDemandExecuted_RecyclerViewBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");

            Prompt.Show("Place a custom marker on the map.");
            _mapGroupManager.ItemAdded += MapCustomObjectAdded;
            MapViewControl.PushMapEvents(MapMouseEvents.MapMouseDown
                                        | MapMouseEvents.MapMouseMove
                                        | MapMouseEvents.MapMouseUp
                                        | MapMouseEvents.ItemDrag
                                        | MapMouseEvents.ItemDragCompleted
                                        | MapMouseEvents.ItemLongPress
                                        | MapMouseEvents.MapDrag
                                        | MapMouseEvents.MapLongPress
                                        | MapMouseEvents.MapDoubleClick
                                        | MapMouseEvents.ItemDoubleClick);
            MapViewControl.MapClick += PlaceCustomHelloWorld_MapClick;
        }
        private void PlaceCustomHelloWorld_MapClick(object sender, MapMouseEventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");

            /* Variables declaration */
            string cotUid;
            string cotType = "c-f-u";
            string cotName = "CHWM"; // HelloWorldMarker
            string cotDetail;

            /* Implementation */
            cotUid = Guid.NewGuid().ToString();
            cotName = _coTManager.CreateCallsign(cotName, CallsignCreationMethod.BasedOnTypeAndDate);
            cotDetail = "<archive /> <_helloworld_ title=\"" + cotName + "\" /><precisionlocation altsrc=\"DTED0\" />";
            TAKEngine.Core.GeoPoint geoPoint;
            geoPoint = new TAKEngine.Core.GeoPoint(e.WorldLocation)
            {
                Altitude = _elevationManager.GetElevation(e.WorldLocation),
                AltitudeRef = global::TAKEngine.Core.AltitudeReference.HAE
            };

            if (double.IsNaN(geoPoint.Altitude))
            {
                geoPoint.Altitude = Altitude.UNKNOWN_VALUE;
            }

            cotGuidGenerate = cotUid;
            _coTManager.AddItem(cotUid, cotType, geoPoint, cotName, cotDetail);

            MapItem value = null; 
            value = CreateCustomMarkerRenderable(value, Guid.NewGuid(), "testCustom", geoPoint);
            Log.i(TAG, "MapItem : " + value.ToString() + "Geopoint : " + geoPoint.ToString());
            AddMapObjectItem(value, geoPoint);
            
            MapViewControl.PopMapEvents();
            Prompt.Clear();
        }
        
        private void MapCustomObjectAdded(object sender, MapItemEventArgs args)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");

            MapItem mapItem;
            mapItem = args?.MapItem;

            if (mapItem == null || cotGuidGenerate == null || cotGuidGenerate == mapItem.GetUid())
            {
                return;
            }
            base.DispatchAsync(delegate
            {
                if (_dockingManager.GetDockPane(ID) is HelloWorldDockPane helloWorldDockPane)
                {
                    MapMarker mapMarker;
                    mapMarker = mapItem.GetMapMarker();
                    if (mapMarker != null)
                    {
                        // helloWorldDockPane.SetMarker(mapMarker);
                        mapItem.Properties.TryGetValue("helloworldMapItem", out var value);
                        if (value != null)
                        {
                            ((MapObjectItem)value).Text = mapItem.Properties["akey?"].ToString();
                        }
                        cotGuidGenerate = null;

                    }

                    //MapItem value = null;
                    //value = CreateCustomMarkerRenderable();
                    //TAKEngine.Core.GeoPoint geoPoint = new TAKEngine.Core.GeoPoint(MapMouseEventArgs.WorldLocation)
                    //{
                    //    Altitude = _elevationManager.GetElevation(MapMouseEventArgs.WorldLocation),
                    //    AltitudeRef = global::TAKEngine.Core.AltitudeReference.HAE
                    //};

                    //AddMapObjectItem(value, geoPoint);
                }
            });
        }
        private void CustomHWOnDisposing(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
            _customMarkerCompositeMapItem.Disposing -= CustomHWOnDisposing;
            _customMarkerCompositeMapItem = null;
        }

        private void AddMapObjectItem(MapItem shape, TAKEngine.Core.GeoPoint center)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
            MapItem mapItem = shape;
            Log.i(TAG, "shape : " + shape.ToString());// + "Parent : " + shape.Parent.ToString());
            Log.i(TAG, "MapItem : " + mapItem.ToString() + "Geopoint : " + center.ToString());
            if (mapItem.Properties.TryGetValue("overlay-manager-entry", out var value))
            {
                ((MapObjectItem)value).Position = center;
                return;
            }

            LegacyMapObjectItem legacyMapObjectItem = new LegacyMapObjectItem(mapItem.GetCallsign(), null);
            legacyMapObjectItem.MapItem = mapItem;
            legacyMapObjectItem.Position = center;
            legacyMapObjectItem.Selectable = false;
            legacyMapObjectItem.Properties["exportable"] = false;
            _customMarker.Children.Add(legacyMapObjectItem);
            mapItem.Properties["overlay-manager-entry"] = legacyMapObjectItem;
        }
        private MapItem CreateCustomMarkerRenderable()
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");

            MapMarker mapMarker = CreateMarker();
            CompositeMapItem compositeMapItem = CreateMapObject(Guid.NewGuid().ToString(), "CustomHW_Test");
            _mapGroup.AddItem(compositeMapItem);

            return CreateMarker();
        }

        private MapItem CreateCustomMarkerRenderable(MapItem shape, Guid id, string title, TAKEngine.Core.GeoPoint center)
        {
            MapMarker mapMarker;
            if (shape == null)
            {
                mapMarker = CreateMarker();
                CompositeMapItem compositeMapItem = CreateMapObject(id.ToString(), title);
                compositeMapItem.AddItem("hwMarker", mapMarker);
                _mapGroup.AddItem(compositeMapItem);
            }
            else
            {
                mapMarker = (MapMarker)shape;
            }
            mapMarker.Position = center;
            return mapMarker;
        }

        private CompositeMapItem CreateMapObject(string id, string callsign = null)
        {
            CompositeMapItem compositeMapItem = new CompositeMapItem();
            compositeMapItem.Properties["uid"] = id;
            compositeMapItem.Properties["callsign"] = callsign ?? id;
            compositeMapItem.Properties["type"] = "h-w-c-m";
            compositeMapItem.Disposing += OnMapObjectDisposed;
            return compositeMapItem;
        }

        private void OnMapObjectDisposed(object sender, EventArgs e)
        {
            MapItem mapItem = (MapItem)sender;
            mapItem.Disposing -= OnMapObjectDisposed;
            Guid id = new Guid(mapItem.Properties["uid"].ToString());
        }
        private static MapMarker CreateMarker()
        {
            return new MapMarker
            {
                Visible = true,
                IconPath = new Uri("pack://application:,,,/Hello World Sample;component/assets/brand_cthulhu.png"),
                Scaling = DisplayManager.UIScale
            };
        }

        
        /* Layout Example - Tab View
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_TabViewBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }

        /* Layout Example - Overlay View
         * --------------------------------------------------------------------
         * Desc. : with an event of mouse over of a marker an image is displayed.
         * */
        private void OnDemandExecuted_OverlayViewBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
            String overlayName = "HelloWorldOverlay";

        }
        
        /* Layout Example - DropDown
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_DropDownBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        // --------------------------------------------------------------------
        // Map Movement
        // --------------------------------------------------------------------
        /* Map Movement - Fly
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_FlyBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        // --------------------------------------------------------------------
        // Marker Manipulation
        // --------------------------------------------------------------------
        /* Marker Manipulation - Special Marker
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_SpecialMarkerButton(object sender, EventArgs e)
        {
            Log.d(TAG, MethodBase.GetCurrentMethod() + "");
            CotEvent cot = new CotEvent
            {
                Uid = Guid.NewGuid().ToString(),
                Type = "a-f-G-U-C-I",
                How = "h-g-i-g-o",
                
            };
            cot.Point.Latitude = 0.0;
            cot.Point.Longitude = 0.0;
            cot.Point.Altitude = 100.0;

            Log.d(TAG, "Testing CoTEvent() : " + cot.ToXml());
            Log.d(TAG, "Testing CoTEvent() : " + cot.ToString());

            Log.d(TAG, "TESTING SOMETHING 1 : " + _locationService.GetGpsPosition().ToString());
            Log.d(TAG, "TESTING SOMETHING 2 : " + _devicePreferences.Callsign);
            Log.d(TAG, "device interface was tested to ensure that we can get something ?");
        }
        
        /* Marker Manipulation - Add an Aircraft
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_AddAnAircraftBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        
        /* Marker Manipulation - Marker with SVG Icon
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_SvgMarkerBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        
        /* Marker Manipulation - Add a Layer
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_AddLayerBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }

        /* Marker Manipulation - Add Multi Layer
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_AddMultiLayerBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }

        /* Marker Manipulation - Add Heat Map
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_AddHeatMapBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }

        /* Marker Manipulation - Stale Out Marker
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_AddStaleOutMarkerBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }

        /* Marker Manipulation - Add Streams
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_AddStreamBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + " enable receiving CoT Message.");
            _cotMessageReceiver.MessageReceived += OnCotMessageReceived;

        }

        private void OnCotMessageReceived(object sender, CoTMessageArgument args)
        {
            Log.d(TAG, "OnCotMessageReceived : " + args.Message.ToString());
            Log.d(TAG, args.CotEvent.ToString());
            Log.d(TAG, args.Type.ToString());
        }
        /* Marker Manipulation - Remove Streams
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_RemoveStreamBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }

        /* Marker Manipulation - Coordiante Entry
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_CoordinateEntryBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }

        /* Marker Manipulation - Map Item (CoT) Inspect
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_ItemInspectBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }

        /* Marker Manipulation - Custome Type
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_CustomTypeBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }

        /* Marker Manipulation - Custom Menu Factory
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_CustomMenuFactoryBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }

        /* Marker Manipulation - ISS Location
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_ISSLocationBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }

        /* Marker Manipulation - Sensor Field of View
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_SensorFOVBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        // --------------------------------------------------------------------
        // Route Examples
        // --------------------------------------------------------------------
        /* Route Examples - List Routes
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_ListRoutesBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }

        /* Route Examples - Add Route
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_AddXRouteBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }

        /* Route Examples - Reroute
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_ReXRouteBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }

        /* Route Examples - Drop Route Example
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_dropRouteBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        // --------------------------------------------------------------------
        // Emergency Examples
        // --------------------------------------------------------------------
        /* Emergency Examples - Emergency
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_EmergencyBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }

        /* Emergency Examples - No Emergency
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_NoEmergencyBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        // --------------------------------------------------------------------
        // Drawing Examples
        // --------------------------------------------------------------------
        /* Drawing Examples - Range/Bearing Circle
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_RbcircleBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        
        /* Drawing Examples - Add Rectangle
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_AddRectangleBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        
        /* Drawing Examples - Draw Shapes Examples
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_DrawShapesBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        
        /* Drawing Examples - Add Shape to Custom Group
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_GroupAddBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
       
        /* Drawing Examples - Associate Two MapItems on the Map
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_AssociationsBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        // --------------------------------------------------------------------
        // GPS Examples
        // --------------------------------------------------------------------
        /* GPS Examples - Simulate External GPS
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_ExternalGpsBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        // --------------------------------------------------------------------
        // Elevation Examples
        // --------------------------------------------------------------------
        /* Elevation Examples - Query Surface Data
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_SurfaceAtCenterBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        // --------------------------------------------------------------------
        // Notification Examples
        // --------------------------------------------------------------------
        private void NotificationExamples_Configuration()
        {
            // Notification Examples - Get Current Notifications
            var getCurrentNotificationsCommand = new ExecutedCommand();
            getCurrentNotificationsCommand.Executed += OnDemandExecuted_GetCurrentNotificationsBtn;
            GetCurrentNotificationsBtn = getCurrentNotificationsCommand;

            // Notification Examples - Fake Content Provider
            var fakeContentProviderCommand = new ExecutedCommand();
            fakeContentProviderCommand.Executed += OnDemandExecuted_FakeContentProviderBtn;
            FakeContentProviderBtn = fakeContentProviderCommand;

            // Notification Examples - Notification Spammer
            var notificationSpammerCommand = new ExecutedCommand();
            notificationSpammerCommand.Executed += OnDemandExecuted_NotificationSpammerBtn;
            NotificationSpammerBtn = notificationSpammerCommand;

            // Notification Examples - Notification with Options
            var notificationWithOptionsCommand = new ExecutedCommand();
            notificationWithOptionsCommand.Executed += OnDemandExecuted_NotificationWithOptionsBtn;
            NotificationWithOptionsBtn = notificationWithOptionsCommand;
            
            // Notification Examples - Notification to WinTak Toast
            var notificationToWinTakToastCommand = new ExecutedCommand();
            notificationToWinTakToastCommand.Executed += OnDemandExecuted_NotificationWinTakToastBtn;
            NotificationToWinTakToastBtn = notificationToWinTakToastCommand;

            // Notification Examples - Notification to Windows
            var notificationToWindowsCommand = new ExecutedCommand();
            notificationToWindowsCommand.Executed += OnDemandExecuted_NotificationToWindows;
            NotificationToWindowsBtn = notificationToWindowsCommand;


        }
        /* Notification Examples - Get Current Notifications
         * --------------------------------------------------------------------
         * Desc. : Get notifications from WinTak
         * */
        private void OnDemandExecuted_GetCurrentNotificationsBtn(object sender, EventArgs e)
        {
            WinTak.Alerts.Notifications.AlertNotification alertNotification;
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
            foreach (WinTak.Framework.Notifications.Notification notification in _notificationLog.Notifications)
            {
                Log.i(TAG, MethodBase.GetCurrentMethod() + " current notification : " + notification.ToString() + " - " + notification.Message);
                // Open another viewer with current notification or display it in the UI. - moved to a service
                //WinTak.UI.Notifications.Notification.NotifyInfo("OnDemandExecuted_GetCurrentNotificationsBtn", notification.ToString() + " - " + notification.Message.ToString());
                //if (notification.GetType() == typeof(WinTak.Alerts.Notifications.AlertNotification))
                //{
                //    alertNotification = (WinTak.Alerts.Notifications.AlertNotification)notification;
                //    Log.d(TAG, "alerNotification.Data : " + alertNotification.Data); // AkertNotificationData
                //    Log.d(TAG, "alertNotification.Uid : " + alertNotification.Uid);
                //    Log.d(TAG, "alertNotification.Type : " + alertNotification.Type); // WinTak.Alerts
                //    Log.d(TAG, "alertNotification.Message : " + alertNotification.Message);
                //    Log.d(TAG, "alertNotification.Key : " + alertNotification.Key); // WinTak.Alerts
                //    Log.d(TAG, "alertNotification.StartTime : " + alertNotification.StartTime);
                //    Log.d(TAG, "alertNotification.GetType() : " + alertNotification.GetType()); // WinTak.Alerts.Notifications.AlertNotification
                    
                //    NotificationData alertData = alertNotification.Data;
                //    Log.d(TAG, "alertData.Action : " + alertData.Action);
                //    Log.d(TAG, "alertData.MessageAction : " + alertData.MessageAction);
                //    Log.d(TAG, "alertData.GetType() : " + alertData.GetType());

                //    alertData.Action += OnDemand_AlertGeofenceAction;
                //    if (alertData.Action != null)
                //    {
                //        IEventAggregator eventAggregator = new EventAggregator(); // Obtain an IEventAggregator instance
                //        alertData.Action(eventAggregator); // Invoke the Action with eventAggregator
                //    }
                //}
            }
            
        }
        private void OnDemand_AlertGeofenceAction(IEventAggregator eventAggregator)
        {
            Log.d(TAG, "Geofence action triggered with Event Aggregator");
            var geofenceAlertEvent = eventAggregator.GetEvent<PubSubEvent<GeofenceAlertMessage>>();
            Log.d(TAG, "geofenceAlertEvent : " + geofenceAlertEvent.ToString());
            geofenceAlertEvent.Subscribe(message =>
            {
                Log.d(TAG, "eventAggregator" + message.FenceUid);
            });
        }

        /* Notification Examples - Fake Content Provider
         * --------------------------------------------------------------------
         * Desc. : Display a simple notification in WinTak
         * */
        private void OnDemandExecuted_FakeContentProviderBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
            
            HelloWorldNotification hwNotification = new HelloWorldNotification
            {
                Uid = ULIDGenerator.GenerateULID(),
                Type = ULIDGenerator.GenerateULID(),
                Key = ULIDGenerator.GenerateULID(),
                StartTime = DateTime.UtcNow,
                StaleTime = DateTime.UtcNow.AddMinutes(1),
                Viewed = false,

            };         
            hwNotification.Message = "Display a WinTAK notification at " + hwNotification.StartTime.ToString();
            _notificationLog.AddNotification(hwNotification);
            Log.i(TAG, MethodBase.GetCurrentMethod() + "Notification start : " + hwNotification.StartTime.ToString() + " / end : " + hwNotification.StaleTime.ToString());

        }

        /* Notification Examples - Notification Spammer
         * --------------------------------------------------------------------
         * Desc. : Loop which send more notification to WinTak
         * */
        private async void OnDemandExecuted_NotificationSpammerBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
            
            for (int i = 0; i < 10; i++)
            {
                HelloWorldNotification hwNotification = new HelloWorldNotification
                { 
                    Uid = ULIDGenerator.GenerateULID(),
                    Type = ULIDGenerator.GenerateULID(),
                    Key = ULIDGenerator.GenerateULID(),
                    StartTime = DateTime.UtcNow,
                    StaleTime = DateTime.UtcNow.AddMinutes(1),
                    Viewed = false,
                    
                };
                hwNotification.Message = "Test Spammer " + (i + 1).ToString() + " - " + hwNotification.StartTime.ToString();
                _notificationLog.AddNotification(hwNotification);

                await Task.Delay(1000);
            }

        }

        /* Notification Examples - Notification with Options
         * --------------------------------------------------------------------
         * Desc. : Notification with a click which focus on map 
         * 
         * */
        // TODO : Notification Examples - Notification with Options : how we can send the user to the localisation of the notification(like range&bearing)
        private void OnDemandExecuted_NotificationWithOptionsBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
            HelloWorldNotification hwNotification = new HelloWorldNotification
            {
                Uid = ULIDGenerator.GenerateULID(),
                Type = ULIDGenerator.GenerateULID(),
                Key = ULIDGenerator.GenerateULID(),
                StartTime = DateTime.UtcNow,
                StaleTime = DateTime.UtcNow.AddMinutes(1),
                Viewed = false,

            };
            hwNotification.Message = "Display a WinTAK notification at " + hwNotification.StartTime.ToString() + " with possibility to focus on the Notification";
            _notificationLog.AddNotification(hwNotification);
            Log.i(TAG, MethodBase.GetCurrentMethod() + "Notification start : " + hwNotification.StartTime.ToString() + " / end : " + hwNotification.StaleTime.ToString());

        }
        
        /* Notification Examples - Notification to WinTak Toast
         * --------------------------------------------------------------------
         * Desc. : Notification with a click which focus on map 
         * 
         * */
        private void OnDemandExecuted_NotificationWinTakToastBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
            string test = "Hello World display a short popup Box on WinTAK.";
            Toast.Show(test);
        }
        /* Notification Examples - Notification to Windows
         * --------------------------------------------------------------------
         * Desc. : Send a notification to Windows Toast (Sidebar notification)
         * */
        private void OnDemandExecuted_NotificationToWindows(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");

            // Example 1 :
            new ToastContentBuilder()
                .AddArgument("action", "viewConversation")
                .AddArgument("conversationId", 9813)
                .AddText("Hello World sent you a picture")
                .AddText("Check this out from WinTak Hello World Plugin")
                .Show();

            // Example 2 :
            Bitmap hwIco = Properties.Resources.ic_launcher_24x24;
            string hwIcoFilePath = SaveImageToFile(hwIco, "ic_launcher_24x24.png") ;
            Uri hwIcoUri = new Uri(hwIcoFilePath);

            Bitmap hwImg = Properties.Resources.hw_notification_icon;
            string hwImgFilePath = SaveImageToFile(hwImg, "hw_notification_icon.png");
            Uri hwImgUri = new Uri(hwImgFilePath);

            new ToastContentBuilder()
                .AddArgument("HelloWorldNotification")
                .AddText("Hello World Plugin Notification with icon and Image")
                .AddAppLogoOverride(hwIcoUri, ToastGenericAppLogoCrop.Circle)
                .AddInlineImage(hwImgUri)
                .AddButton(new ToastButton().SetContent("Acknowledge")
                .AddArgument("HelloWorldNotificationAck"))
                .Show(toast =>
                {
                    toast.Tag = "HelloWordNotification";
                });
        }

        /* Notification Examples - Play a Video with Overlay
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_VideoLauncherBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        
        /* Notification Examples - Add Toolbar Item
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_AddToolbarItemBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        
        /* Notification Examples - Add Count to Icon
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_AddCountToIconBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        // --------------------------------------------------------------------
        // Images and Camera
        // --------------------------------------------------------------------
        /* Images and Camera - Launch a Camera
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_CameraLauncherBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        /* Images and Camera - Image Attach
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_ImageAttachBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        /* Images and Camera - Web View
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_WebViewBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        /* Images and Camera - Map Screenshot
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_MapScreenshotBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        // --------------------------------------------------------------------
        // Speach To Text
        // --------------------------------------------------------------------
        /* Speach To Text - Speech To Text
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_SpeechToTextBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        /* Speach To Text - Speech To Activity
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_SpeechToActivityBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        // --------------------------------------------------------------------
        // Sensors
        // --------------------------------------------------------------------
        /* Sensors - Acceleromter Control Test
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_BumpControlBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        // --------------------------------------------------------------------
        // Navigation
        // --------------------------------------------------------------------
        /* Navigation - Hook into navigation events
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_HookNavigationEventsNameBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        // --------------------------------------------------------------------
        // Lower Level Examples
        // --------------------------------------------------------------------
        /* Lower Level Examples - Get Image
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_GetImagesBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        // --------------------------------------------------------------------
        // Map Layers
        // --------------------------------------------------------------------
        /* Map Layers - Download Map Layer
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_DownloadMapLayerBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }
        // --------------------------------------------------------------------
        // Spinner Examples
        // --------------------------------------------------------------------
        /* Spinner Examples - 
         * --------------------------------------------------------------------
         * Desc. :
         * */
        private void OnDemandExecuted_Spinner1Btn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
        }

        // --------------------------------------------------------------------
        // Plugin Template Duplicate
        // --------------------------------------------------------------------
        /* Plugin Template Duplicate - Counter
         * --------------------------------------------------------------------
         * Desc. :
         * */
        // This method is linked to the TextBlock where the counter value is displayed.
        public int Counter
        {
            get { return _counter; }
            set { SetProperty(ref _counter, value); }
        }
        // This method is linked to the Button when an OnClick() is done.
        private void OnDemandExecuted_IncreaseCounterBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
            Counter++;
        }

        /* Plugin Template Duplicate - (de)activate
         * --------------------------------------------------------------------
         * Desc. : This is an example of how to interact with the MapComponent
         *         on some part.
         * */
        public bool MapFunctionIsActivate
        {
            get { return _mapFunctionIsActivate; } 
            set
            {
                SetProperty<bool>(ref _mapFunctionIsActivate, value, "MapFunctionIsActive");
                if (value)
                {
                    MapViewControl.MapMouseMove += MapViewControl_MapMouseMove;
                   
                } else
                {
                    MapViewControl.MapMouseMove -= MapViewControl_MapMouseMove;
                }
            }
        }
        public double MapFunctionLon
        {
            get { return _mapFunctionLon;  }
            set { SetProperty(ref _mapFunctionLon, value); }
        }
        public double MapFunctionLat
        {
            get { return _mapFunctionLat; }
            set { SetProperty(ref _mapFunctionLat, value) ; }
        }

        private void MapViewControl_MapMouseMove(object sender, MapMouseEventArgs e)
        {
            MapFunctionLat = e.WorldLocation.Latitude;
            MapFunctionLon = e.WorldLocation.Longitude;
        }

        /* Plugin Template Duplicate - White House
         * --------------------------------------------------------------------
         * Desc. : Client code can programmatically request WinTAK to focus 
         *         on a GeoPoint(s) (pan and zoom to). This and other actions 
         *         can be achieved with the WinTak.Framework.Messaging.IMessageHub 
         *         pub/sub interface.    
         * */
        private void OnDemandExecuted_WhiteHouseCoTBtn(object sender, EventArgs e)
        {
            Log.i(TAG, MethodBase.GetCurrentMethod() + "");
            
            var message = new FocusMapMessage(new TAKEngine.Core.GeoPoint(38.8977, -77.0365)) { Behavior = WinTak.Common.Events.MapFocusBehavior.PanOnly };
            _messageHub.Publish(message);
        }
    }
}
