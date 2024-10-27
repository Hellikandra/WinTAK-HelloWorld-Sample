using ElevationTools.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinTak.Location.Providers;
using WinTak.Common.Location;
using WinTak.Common.Conversion;
using WinTak.Common.Properties;
using WinTak.Location.Preferences;
using TAKEngine.Core;
using TAKEngine.Elevation;
using System.Threading;
using Windows.ApplicationModel.VoiceCommands;
using System.Windows;
using MapEngine.Interop.Util;

namespace Hello_World_Sample
{
    [Export(typeof(ILocationProvider))]
    internal class HelloWorldLocationProvider : LocationProvider
    {
        private GeoPoint _geoPoint;
        private Boolean run;
        private Thread thread;

        private IHelloWorldLocationPreferences _preferences;
        internal IHelloWorldLocationPreferences Preferences
        {
            get
            {
                return this._preferences;
            }
            set
            {
                this._preferences = value;
            }
        }

        [ImportingConstructor]
        public HelloWorldLocationProvider()
        {
            Log.d("HelloWorldLocationProvider", "HelloWorldLocationProvider() : test");
            base.Title = "Hello World Location Provider";
            base.Id = "HelloWorldLocationProvider";
            base.Type = WinTak.Common.Location.GpsType.Developer;

            //base.PositionUpdated += new PositionUpdatedEventArgs(new GeoPoint(50, 5), FixQuality.Simulated, 0);
            //StartAsync();
        }
        private void OnStartingLocationChanged(object sender, string s)
        {
            Log.d("HelloWorldLocationProvider", "OnStartingLocationChanged() : test : " + s);
            this.UpdatePosition(s);
        }
        private void UpdatePosition(string positionMgrs)
        {
            Log.d("HelloWorldLocationProvider", "UpdatePosition() : test");
            GeoPoint geoPoint = CoordinateFormatUtilities.Convert(positionMgrs, CoordinateFormat.MGRS);
            // 32TNP5519698580 (MGRS) = 44.2383660°, 009.6912326° (Degrees Lat Long)
            this._geoPoint = ((geoPoint != null) ? new GeoPoint(44.238366, 9.6912326) : null);
            
            if (this._geoPoint != null)
            {
                this._geoPoint.Altitude = ElevationManager.getElevation(this._geoPoint.Latitude, this._geoPoint.Longitude, null);
            }
        }
        public override Task StartAsync()
        {
            Log.d("HelloWorldLocationProvider", "StartAsync() : test");
            Log.d("HelloWorldLocationProvider", "Test Preferences : " + Preferences);
            if (Preferences != null)
            {
                if (!string.IsNullOrEmpty(Preferences.StartingLocation))
                {
                    UpdatePosition(Preferences.StartingLocation);
                }
                Preferences.add_PropertyChanged(OnStartingLocationChanged);
                if (this.thread == null || !this.thread.IsAlive)
                {
                    this.thread = new Thread(SimulationThread)
                    {
                        IsBackground = true,
                        Name = "HelloWorldLocationProvider_thread"
                    };
                    this.run = true;
                    this.thread.Start();
                }
            }
            return Task.CompletedTask;
        }

        public override void Stop()
        {
            if (Preferences != null)
            {
                Preferences.remove_PropertyChanged(OnStartingLocationChanged);
                {
                    if (this.thread != null && this.thread.IsAlive)
                    {
                        this.run = false;
                        this.thread.Interrupt();
                        this.thread.Join();
                    }
                }
            }
        }

        private void SimulationThread()
        {
            while (this.run && Application.Current != null)
            {
                try
                {
                    if (this._geoPoint != null)
                    {
                        _ = this._geoPoint;

                        OnPositionUpdated(new PositionUpdatedEventArgs(this._geoPoint, FixQuality.Simulated, 6.0, Preferences.Speed, Preferences.Bearing));
                        Log.d("HelloWorldLocationProvider", "SimulationThread() : test");
                    }
                    Thread.Sleep(Preferences.UpdateRate);
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
