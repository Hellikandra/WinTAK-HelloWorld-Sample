using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;
using System.Windows.Input;
using System.Windows;
using WinTak.Framework.Docking;
using WinTak.Framework.Docking.Attributes;
using MapEngine.Interop.Util;
using WinTak.Display;
using WinTak.Common.Preferences;
using WinTak.Common.CoT;
using WinTak.Common.Services;
using WinTak.Framework;
using WinTak.CursorOnTarget.Services;

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

        // Layout Example
        public ICommand LargerButton { get; private set; }
        public ICommand SmallerButton { get; private set; }
        public IDockingManager _dockingManager;
        private ICotMessageSender _cotMessageSender;
        
        
        // Marker Manipulation
        public ICommand SpecialMarkerButton { get; private set; }
        public IDevicePreferences _devicePreferences;
        public ILocationService _locationService;


        // Plugin Template Duplicate (From WinTAK-Documentation)
        private int _counter;
        public ICommand IncreaseCounterButton { get; private set; }

        private double _mapFunctionLat;
        private double _mapFunctionLon;
        private bool _mapFunctionIsActivate;

        // ----- CONSTRUCTOR -----//
        [ImportingConstructor] // this import provide the capability to get WinTAK exposed Interface
        public HelloWorldDockPane(IDockingManager dockingManager, ILogger logger, ILocationService service, IDevicePreferences devicePreferences, ICotMessageSender cotMessageSender)
        {
            /* Show messages in the log files in the %APPDATA%/roaming/WinTAK/Logs folder */
            /* There is logs files generated depending of the type of Logs recorded in the plugin */
            _logger = logger;
            _logger.Info("Test Info Message from HelloWorldDockPane");
            _logger.Debug("Test Debug Message from HelloWorldDockPane");
            _logger.Warn("Test Warn Message from HelloworldDockPane");
            _logger.Error("Test Error Message from HelloworldDockPane");
            _logger.Fatal("Test Fatal Message from HelloworldDockPane");
            
            _dockingManager = dockingManager;
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
            _cotMessageSender = cotMessageSender;

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
            Size dockPaneFloat = new Size(1024,368);
            Point dockPanePoint = new Point(0,120);
            this.Float(dockPaneFloat, dockPanePoint, true); // does not work            
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
                How = "h-g-i-g-o"
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
