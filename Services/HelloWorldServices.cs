using Hello_World_Sample.Geofences;
using MapEngine.Interop.Util;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinTak.Alerts.Notifications;
using WinTak.Common.Geofence;
using WinTak.Framework.Notifications;
using WinTak.Overlays.Services;
using WinTak.Overlays.ViewModels;

namespace Hello_World_Sample.Services
{
    [Export(typeof(IHelloWorldServices))]
    internal class HelloWorldServices : IHelloWorldServices, IDisposable
    {
        internal const string TAG = "HelloWorldServices";

        private readonly IGeofenceManager _geofenceManager;
        private readonly IMapObjectItemManager _mapObjectItemManager;
        private readonly INotificationLog _notificationLog;
        public GeofencesMonitoring geofencesMonitoring { get; set; }

        [ImportingConstructor]
        public HelloWorldServices(IGeofenceManager geofenceManager, IMapObjectItemManager mapObjectItemManager, INotificationLog notificationLog)
        {
            Log.d(TAG, "Init the services");

            // Listing Geo Fences
            _geofenceManager = geofenceManager;
            _geofenceManager.GetGeofences();
            foreach(GeofenceData geofenceData in geofenceManager.GetGeofences())
            {
                Log.d(TAG, "Geofence: " + geofenceData.MapItemUid);
            }

            _geofenceManager.GeofenceAdded += GeofenceManager_GeofenceAdded;
            _geofenceManager.GeofenceRemoved += GeofenceManager_GeofenceRemoved;
            _geofenceManager.GeofenceChanged += GeofenceManager_GeofenceChanged;


            // Listing Map Items - Linked to the Geo Fences
            _mapObjectItemManager = mapObjectItemManager;
            ICollection<MapObjectItem> mapObjectItems = _mapObjectItemManager.RootItems;
            foreach (MapObjectItem mapObjectItem in mapObjectItems)
            {
                if (mapObjectItem.Text is "Geo Fences")
                {
                    //GeofenceMapObjectItem geofenceMapObjectItem = (GeofenceMapObjectItem)mapObjectItem;
                    //Log.d(TAG, "MapItem: " + geofenceMapObjectItem.GeofenceData.MapItemUid);
                }
            }

            // Notification Manager
            _notificationLog = notificationLog;
            notificationLog.NotificationAdded += NotificationLog_NotificationAdded;


        }

        private void NotificationLog_NotificationAdded(object sender, Notification e)
        {
            AlertNotification alertNotification;
            Log.d(TAG, "NotificationAdded : " + e.ToString());
            // The idea is to get only the AlertNotification which is used when a geo fence is breach
            // This event is only called when a new Notification appears.
            
            if (e.GetType() == typeof(AlertNotification))
            {
                alertNotification = (AlertNotification)e;
                Log.d(TAG, "alerNotification.Data : " + alertNotification.Data); // AkertNotificationData
                Log.d(TAG, "alertNotification.Uid : " + alertNotification.Uid);
                Log.d(TAG, "alertNotification.Type : " + alertNotification.Type); // WinTak.Alerts
                Log.d(TAG, "alertNotification.Message : " + alertNotification.Message);
                Log.d(TAG, "alertNotification.Key : " + alertNotification.Key); // WinTak.Alerts
                Log.d(TAG, "alertNotification.StartTime : " + alertNotification.StartTime);
                Log.d(TAG, "alertNotification.GetType() : " + alertNotification.GetType()); // WinTak.Alerts.Notifications.AlertNotification

                NotificationData alertData = alertNotification.Data;
                Log.d(TAG, "alertData.Action : " + alertData.Action);
                Log.d(TAG, "alertData.MessageAction : " + alertData.MessageAction);
                Log.d(TAG, "alertData.GetType() : " + alertData.GetType());

                alertData.Action += OnDemand_AlertGeofenceAction;
                if (alertData.Action != null)
                {
                    IEventAggregator eventAggregator = new EventAggregator(); // Obtain an IEventAggregator instance
                    alertData.Action(eventAggregator); // Invoke the Action with eventAggregator
                }
            }
        }

        private void OnDemand_AlertGeofenceAction(IEventAggregator eventAggregator)
        {
            Log.d(TAG, "Geofence action triggered with Event Aggregator");
            //var geofenceAlertEvent = eventAggregator.GetEvent<PubSubEvent<GeofenceAlertMessage>>();
            //Log.d(TAG, "geofenceAlertEvent : " + geofenceAlertEvent.ToString());
            //geofenceAlertEvent.Subscribe(message =>
            //{
            //    Log.d(TAG, "eventAggregator" + message.FenceUid);
            //});
            ICollection<MapObjectItem> rootItems = _mapObjectItemManager.RootItems;
            foreach (MapObjectItem mapObjectItem in rootItems)
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
                        if (moi.SubText != "N/A")
                        {
                            Log.d(TAG, "this Geo Fence seems to be breached !");
                            ObservableCollection<MapObjectItem> moiChildrens = moi.Children;
                            
                            foreach (MapObjectItem moiChildren in moiChildrens)
                            {
                                Log.d(TAG, "Children Text :" + moiChildren.Text);
                                Log.d(TAG, "Children Show Settings : " + moiChildren.ShowSettings);
                                Log.d(TAG, "Children ToolTip : " + moiChildren.ToolTip);
                                Log.d(TAG, "Children Id : " + moiChildren.Id);
                                Log.d(TAG, "Children Properties : " + moiChildren.Properties);
                                Dictionary<String, Object> moiChildrenProps = moiChildren.Properties;
                                // Iterate through the dictionary and print each key-value pair
                                foreach (KeyValuePair<string, object> kvp in moiChildrenProps)
                                {
                                    Log.d(TAG, "Key: " + kvp.Key + ", Value: " + kvp.Value);
                                }
                                Log.d(TAG, "Children SubText  : " + moiChildren.SubText);
                                Log.d(TAG, "Children " + moiChildren.ShowDetailsCommand);

                            }
                        }
                        Log.d(TAG, "/---------------------------------------------/");
                    }

                }
            }
        }

        public void Dispose()
        {
            Log.d(TAG, "Dispose the services");
        }

        // Geofence Events - Geofence is updated
        private void GeofenceManager_GeofenceChanged(object sender, GeofenceData gd)
        {
            Log.d(TAG, "GeofenceChanged : " + gd.MapItemUid + "\n"
                     + "\tTracking           : " + gd.Tracking + "\n"
                     + "\tTrigger            : " + gd.Trigger + "\n"
                     + "\tMonitoredType      : " + gd.MonitoredType + "\n"
                     + "\tRange              : " + gd.Range + "\n"
                     + "\tElevationMonitored : " + gd.ElevationMonitored + "\n"
                     + "\tMinElevation       : " + gd.MinElevation + "\n"
                     + "\tMaxElevation       : " + gd.MaxElevation + "\n");
        }

        private void GeofenceManager_GeofenceRemoved(object sender, GeofenceData gd)
        {
            Log.d(TAG, "GeofenceRemoved: " + gd.MapItemUid + "\n"
                     + "\tTracking           : " + gd.Tracking + "\n"
                     + "\tTrigger            : " + gd.Trigger + "\n"
                     + "\tMonitoredType      : " + gd.MonitoredType + "\n"
                     + "\tRange              : " + gd.Range + "\n"
                     + "\tElevationMonitored : " + gd.ElevationMonitored + "\n"
                     + "\tMinElevation       : " + gd.MinElevation + "\n"
                     + "\tMaxElevation       : " + gd.MaxElevation + "\n");
        }

        private void GeofenceManager_GeofenceAdded(object sender, GeofenceData gd)
        {
            Log.d(TAG, "GeofenceAdded: " + gd.MapItemUid + "\n"
                     + "\tTracking           : " + gd.Tracking + "\n"
                     + "\tTrigger            : " + gd.Trigger + "\n"
                     + "\tMonitoredType      : " + gd.MonitoredType + "\n"
                     + "\tRange              : " + gd.Range + "\n"
                     + "\tElevationMonitored : " + gd.ElevationMonitored + "\n"
                     + "\tMinElevation       : " + gd.MinElevation + "\n"
                     + "\tMaxElevation       : " + gd.MaxElevation + "\n");
        }

    }
}
