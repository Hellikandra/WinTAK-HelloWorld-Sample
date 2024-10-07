using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data.Common;
using System.Windows;
using System.Windows.Media;
using Hello_World_Sample.Services;
using MapEngine.Interop.Util;
using Prism.Mef.Modularity;
using Prism.Modularity;
using WinTak.Display;
using WinTak.Display.Controls;
using WinTak.Framework.Docking;
using WinTak.Graphics;
using WinTak.UI.Themes;

namespace Hello_World_Sample
{
    [ModuleExport(typeof(HelloWorldModule), InitializationMode = InitializationMode.WhenAvailable)]
    /* If you have services in your plugin that need to be initialized at startup, it's best to start those services from a Module
     * */
    internal class HelloWorldModule : IModule
    {
        internal const string TAG = "HelloWorldModule";

        private IDockingManager _dockingManager;
        // Define the number of wheelMenuItem needed here -
        private WheelMenuItem _detailsWheelItem;
        private WheelMenuItem _detailsWheelItem2;
        private WheelMenu _wheelMenuGeneral;
        private IMapViewController _mapViewController;
        
        private readonly IHelloWorldServices _helloWorldServices;
        
        [ImportingConstructor]
        public HelloWorldModule(IDockingManager dockingManager, IHelloWorldServices helloWorldServices, IMapViewController mapViewController)
        {
            _dockingManager = dockingManager;
            _helloWorldServices = helloWorldServices;
            _mapViewController = mapViewController;
        }
        // Modules will be initialized during startup. Any work that needs to be done at startup can
        // be initiated from here.
        public async void Initialize() {

            MapViewControl.WheelMenuOpening += MapViewControl_OnWheelMenuOpening;

            // Test 1            
            ObservableCollection<WheelMenuItemBase> items = new ObservableCollection<WheelMenuItemBase>();

            _detailsWheelItem = new WheelMenuItem("brand_cthulhu", Application.Current.Resources[Icons.BreadcrumbActiveRadialMenuIconKey] as ImageSource, OnDetailsWheelItemClick)
            {
                Id = WinTak.Common.Properties.Resources.AddGeofence,
                ToolTip = "Test Sub Items Menu",
            };
            // Sub items in brand_cthulhu
            items.Add(new WheelMenuItem("", Application.Current.Resources[Icons.CameraRadialMenuIconKey] as ImageSource, OnDetailsWheelItemClick)
            {
                Id = WinTak.Common.Properties.Resources.Details,
                ToolTip = "Details",
            });
            items.Insert(0, new WheelMenuItem("", Application.Current.Resources[Icons.FieldOfViewDirectionRadialMenuIconKey] as ImageSource, OnDetailsWheelItemClick)
            {
                Id = WinTak.Common.Properties.Resources.About,
                ToolTip = "About",
            });
            foreach (var item in items)
            {
                _detailsWheelItem.AddSubMenuItem(item);
            }

            Log.d(TAG, "_details_WheelItem.Id : " + _detailsWheelItem.Id);
            Log.d(TAG, "_details_WheelItem.Text : " + _detailsWheelItem.Text);
            Log.d(TAG, "_details_WheelItem.ToolTip : " + _detailsWheelItem.ToolTip);
            Log.d(TAG, "_details_WheelItem.Icon : " + _detailsWheelItem.Icon);
            Log.d(TAG, "_details_WheelItem.Image : " + _detailsWheelItem.Image);
            Log.d(TAG, "_details_WheelItem.Owner : " + _detailsWheelItem.Owner);
            Log.d(TAG, "_detailsWheelItem.BackColor : " + _detailsWheelItem.BackColor);
            Log.d(TAG, "_detailsWheelItem.PassClickUp : " + _detailsWheelItem.PassClickUp);
            Log.d(TAG, "_detailsWheelItem.ShowSubMenuItems : " + _detailsWheelItem.ShowSubMenuItems);
            Log.d(TAG, "_detailsWheelItem.SubItems : " + _detailsWheelItem.SubItems);
            Log.d(TAG, "_detailsWheelItem.Tag : " + _detailsWheelItem.Tag);

            // Test 2
            _detailsWheelItem2 = new WheelMenuItem("brand_cthulhu_2", Application.Current.Resources[Icons.GeofenceRadialMenuIconKey] as ImageSource, OnDetailsWheelItemClick)
            {
               Id = WinTak.Common.Properties.Resources.Details,
               ToolTip = "Direct Add SubItem ?",
            };
        }
        private void WheelMenu_Click_Event(object sender, EventArgs e)
        {
            Log.d(TAG, "WheelMenu_Click_Event() start");
            Log.d(TAG, "Sender Type: " + (sender?.GetType().ToString() ?? "null"));
            Log.d(TAG, "WheelMenu Clicked");
        }

        private void MapViewControl_OnWheelMenuOpening(object sender, MenuPopupEventArgs e)
        {
            MapEngine.Interop.Util.Log.d(TAG, "OnWheelMenuOpening() start");
            if (e.ClickedMapItems != null && e.ClickedMapItems.Count == 1)
            {
                WheelMenu val = (WheelMenu)sender;
                MapItem parent = e.ClickedMapItems[0].Parent;
                if (parent != null && parent.Properties.ContainsKey("type") && parent.Properties["type"].ToString() == "h-w-c-m")
                {
                    ((WheelMenuItemBase)_detailsWheelItem).Tag = parent;

                    // Add the number of item to insert here.
                    val.Items.Insert(0, _detailsWheelItem);
                    val.Items.Insert(1, _detailsWheelItem2);
                }

            }
        }
        private void OnDetailsWheelItemClick(object sender, EventArgs e)
        {
            MapEngine.Interop.Util.Log.d(TAG, "OnDetailsWheelItemClick() start");
            
            Log.d(TAG, "Sender Type: " + (sender?.GetType().ToString() ?? "null"));

            // Do something when the WheelMenuItem is clicked
            object tag = ((WheelMenuItemBase)(WheelMenuItem)sender).Tag;
            MapItem val = (MapItem)((tag is MapItem) ? tag : null);
            if(((WheelMenuItem)sender).Tag is MapItem mapItem && mapItem.Properties.ContainsKey("uid"))
            {
                Guid id = new Guid(mapItem.Properties["uid"].ToString());
                ShowDockPane(id);
            }
        }
        private async void ShowDockPane(Guid id)
        {
            //await ((HelloWorldDockPane)_dockingManager.GetDockPane("HelloWorld_HelloWorldDockPane")).ShowDockPaneAsync();
        }
    }
}
