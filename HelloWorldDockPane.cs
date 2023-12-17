using System;
using System.Linq;
using System.Reflection;

using System.ComponentModel.Composition;
using System.Windows.Input;
using System.Windows;

using MapEngine.Interop.Util;

using WinTak.Display;
using WinTak.Common.Preferences;
using WinTak.Common.CoT;
using WinTak.Common.Services;
using WinTak.Framework;
using WinTak.Framework.Docking;
using WinTak.Framework.Docking.Attributes;
using WinTak.Framework.Messaging;
using WinTak.Framework.Notifications;
using WinTak.Common.Messaging;

using Hello_World_Sample.Notifications;
using Hello_World_Sample.Common;

namespace Hello_World_Sample
{
    [DockPane(ID, "HelloWorld", Content = typeof(HelloWorldView))]
    /* The DockPane class provides the ability to Hide/Show your dockable window 
     * and serves as the ViewModel for your view.
     * */
    internal class HelloWorldDockPane : DockPane
    {

        internal const string ID  = "HelloWorld_HelloWorldDockPane";
        internal const string TAG = "HelloWorldDockPane";

        public ILogger _logger;

        public INotificationLog _notificationLog;
        // Layout Example
        public ICommand LargerButton { get; private set; }
        public ICommand SmallerButton { get; private set; }
       
        public IDockingManager DockingManager { get; set; }

        /* Set for all DockPane Panel a new width. It is an after-action variable. Does not dynamically change something
         * However, if you change it in the application, it will set the value to another one.
         * */
        [Export("DefaultWindowWidth")]
        public double DefaultWindowWidth;
        //public IDockPaneMetadata _dockPaneMetadata; // The plugin is not launch when the IDockPaneMetadata is set as input in the constructor.

        
        
        // Marker Manipulation
        public ICommand SpecialMarkerButton { get; private set; }
        public IDevicePreferences _devicePreferences;
        public ILocationService _locationService;

        private readonly IMessageHub _messageHub;


        // Plugin Template Duplicate (From WinTAK-Documentation)
        private int _counter;
        public ICommand IncreaseCounterButton { get; private set; }

        private double _mapFunctionLat;
        private double _mapFunctionLon;
        private bool _mapFunctionIsActivate;


        // ----- CONSTRUCTOR -----//
        [ImportingConstructor] // this import provide the capability to get WinTAK exposed Interface
        public HelloWorldDockPane(
            IDockingManager dockingManager,
            ILogger logger,
            ILocationService service,
            IDevicePreferences devicePreferences,
            IMessageHub messageHub,
            INotificationLog notificationLog)
        {

            _logger = logger;
            _notificationLog = notificationLog;
            foreach (Notification notification in notificationLog.Notifications)
            {
                _logger.Info("HERE IS A NOTIFICATION : " + notification.ToString());
            }

            HelloWorldNotification hwNotifiction = new HelloWorldNotification();
            hwNotifiction.Uid = "unique_1";
            hwNotifiction.Message = "test a notification with HelloWorld";
            _notificationLog.AddNotification(hwNotifiction);
            


            //Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .ToArray();

            _logger.Debug("UNDER TEST TO DETERMINE INFORMATION");
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    var exportedInterfaces = assembly.GetExportedTypes().Where(type => type.IsInterface && type.IsDefined(typeof(InheritedExportAttribute)));
                    var importedInterfaces = assembly.GetExportedTypes().SelectMany(type => type.GetProperties().Where(prop => prop.PropertyType.IsInterface && prop.IsDefined(typeof(ImportAttribute)))).Select(prop => prop.PropertyType).Distinct();

                    foreach (var exportedInterface in exportedInterfaces)
                    {
                        _logger.Debug("EXPORTED INTERFACES: " + exportedInterface.FullName);
                    }

                    foreach (var importedInterface in importedInterfaces)
                    {
                        _logger.Debug("IMPORTED INTERFACES: " + importedInterface.FullName);
                    }
                } catch (ReflectionTypeLoadException ex)
                {
                    foreach (Exception innerEx in ex.LoaderExceptions)
                    {
                        _logger.Error("Error loading assembly: {innerEx.Message}");
                    }
                } catch (Exception ex)
                {
                    _logger.Error(ex.ToString());
                }
            }
  

            //_logger.Info("test, " + _alert);
            /* Show messages in the log files in the %APPDATA%/roaming/WinTAK/Logs folder */
            /* There is logs files generated depending of the type of Logs recorded in the plugin */
            
            
            DockingManager = dockingManager;
            _locationService = service;
            _devicePreferences = devicePreferences;
  
            // Layout Example - Larger
            var largerCommand = new ExecutedCommand();
            largerCommand.Executed += OnDemandExecuted_LargerButton;
            LargerButton = largerCommand;

            // Layout Example - Smaller
            var smallerCommand = new ExecutedCommand();
            smallerCommand.Executed += OnDemandExecuted_SmallerButton;
            SmallerButton = smallerCommand;


            // Marker Manipulation - Special Marker
            var specialMarkerCommand = new ExecutedCommand();
            specialMarkerCommand.Executed += OnDemandExecuted_SpecialMarkerButton;
            SpecialMarkerButton = specialMarkerCommand;

            
            _messageHub = messageHub;
            

            // Plugin Template Duplicate (From WinTAK-Documentation)
            var counterButtonCommand = new ExecutedCommand();
            counterButtonCommand.Executed += OnDemandeExecuted_IncreaseCounterButton;
            IncreaseCounterButton = counterButtonCommand;
        }

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
        /* Layout Example
         * --------------
         * Larger button is to Float the Dockpane. 
         * */
        private void OnDemandExecuted_LargerButton(object sender, EventArgs e)
        {
            _logger.Info("OnDemandExecuted_LargerButton : " + "Float the DockPane.");

            // Testing IAlert
            
            // Testing the Float from DockPane
            Size dockPaneFloat = new Size(1024,368);
            Point dockPanePoint = new Point(0,120);

            this.Float(dockPaneFloat, dockPanePoint, true); // does not work

            // Testing the DefaultWindowWidth { set; get; }
            // value is set but seems not working ?
            _logger.Debug("OnDemandExecuted_LargerButton : From DockingManager : " + DockingManager.DefaultWindowWidth);
            _logger.Debug("OnDemandExecuted_LargerButton : From Plugin Information : " + this.DefaultWindowWidth);
            DockingManager.DefaultWindowWidth = 500;
            _logger.Debug("OnDemandExecuted_LargerButton : After set Plugin Information : " + this.DefaultWindowWidth);
            this.DefaultWindowWidth = DockingManager.DefaultWindowWidth;
            _logger.Debug("OnDemandExecuted_LargerButton : Set to Docking Manager : " + DockingManager.DefaultWindowWidth);

            // Testing to get Device Preferences
            _logger.Info("Testing the DevicePreferences : " + _devicePreferences.Callsign);

            // The Uid shall be Unique to ensure that you can stack it
            // The Notification need to be a single notification. You need to create a new one if you want to use it again ?
            // Or is it releated to Type ? Key ? something else ?
            // Seems that is not the Uid how manage the possibility to have one or more Notification, but more the new ?
            HelloWorldNotification hwNotifiction = new HelloWorldNotification();
            hwNotifiction.Uid = ULIDGenerator.GenerateULID();
            hwNotifiction.StartTime = DateTime.UtcNow;
            hwNotifiction.Message = "1. test a notification with HelloWorld by clicking on a button";
            _notificationLog.AddNotification(hwNotifiction);
            
            hwNotifiction.Type = ULIDGenerator.GenerateULID(); // can be used to add a Notification information but we cannot see the first information if only type
            hwNotifiction.Key = ULIDGenerator.GenerateULID();  
            hwNotifiction.Uid = ULIDGenerator.GenerateULID();
            hwNotifiction.StartTime = DateTime.UtcNow;
            hwNotifiction.Message = "2. test a notification with HelloWorld by clicking on a button";
            _notificationLog.AddNotification(hwNotifiction);

            HelloWorldNotification hwNotifiction_2 = new HelloWorldNotification();
            hwNotifiction_2.Uid = ULIDGenerator.GenerateULID();
            hwNotifiction_2.StartTime = DateTime.UtcNow;
            hwNotifiction_2.Message = "test 2 for HW by clicking";
            _notificationLog.AddNotification(hwNotifiction_2);
            // AddNotification is not a stack notifications.
            // How we can stack it ?

        }

        /* Client code can programmatically request WinTAK to focus on a GeoPoint(s) (pan and zoom to). 
         * This and other actions can be achieved with the WinTak.Framework.Messaging.IMessageHub pub/sub interface.
         * */
        private void PanToWhiteHouse()
        {
            var message = new FocusMapMessage(new TAKEngine.Core.GeoPoint(38.8977, -77.0365)) { Behavior = WinTak.Common.Events.MapFocusBehavior.PanOnly };
            _messageHub.Publish(message);
        }

        /* Smaller button is to Hide the Dockpane.
         * */
        private void OnDemandExecuted_SmallerButton(object sender, EventArgs e)
        {
            _logger.Info("OnDemandExecuted_SmallerButton : " + "Hide the DockPane.");
            this.Hide();
        }

        private void OnDemandExecuted_ShowSearchIcon(object sender, EventArgs e)
        {
            _logger.Info("OnDemandExecuted_ShowSearchIcon : ");

        }
        // Marker Manipulation - Special Marker
        // -------------------   --------------
        private void OnDemandExecuted_SpecialMarkerButton(object sender, EventArgs e)
        {
            Log.d(TAG, "OnDemandExecuted_SpecialMarkerButton" + e);
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
        // --------------------------
        // Plugin Template Duplicate
        // --------------------------
        // This method is linked to the TextBlock where the counter value is displayed.
        public int Counter
        {
            get { return _counter; }
            set { SetProperty(ref _counter, value); }
        }
        // This method is linked to the Button when an OnClick() is done.
        private void OnDemandeExecuted_IncreaseCounterButton(object sender, EventArgs e)
        {
            Counter++;
        }
        // This is an example of how to interact with the MapComponent on some part.
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
    }
}
