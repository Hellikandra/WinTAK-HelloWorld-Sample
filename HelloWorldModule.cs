using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using Hello_World_Sample.Services;
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
        private IDockingManager _dockingManager;
        private WheelMenuItem _detailsWheelItem;

        private readonly IHelloWorldServices _helloWorldServices;
        [ImportingConstructor]
        public HelloWorldModule(IDockingManager dockingManager, IHelloWorldServices helloWorldServices) 
        { 
            _dockingManager = dockingManager;
            _helloWorldServices = helloWorldServices;
        }
        // Modules will be initialized during startup. Any work that needs to be done at startup can
        // be initiated from here.
        public async void Initialize() { 
            
            MapViewControl.WheelMenuOpening += MapViewControl_OnWheelMenuOpening;
            // Create a new WheelMenuItem
            _detailsWheelItem = new WheelMenuItem("", Application.Current.Resources[Icons.DetailsRadialMenuIconKey] as ImageSource, OnDetailsWheelItemClick)
            {
                Id = WinTak.Common.Properties.Resources.Details,
                ToolTip = "Details",
            };

        }

        private void MapViewControl_OnWheelMenuOpening(object sender, MenuPopupEventArgs e)
        {
            // Add the WheelMenuItem to the WheelMenu
            
        }
        private void OnDetailsWheelItemClick(object sender, EventArgs e)
        {
            // Do something when the WheelMenuItem is clicked
            if(((WheelMenuItem)sender).Tag is MapItem mapItem && mapItem.Properties.ContainsKey("uid"))
            {
                Guid id = new Guid(mapItem.Properties["uid"].ToString());
                ShowDockPane(id);
            }
        }
        private async void ShowDockPane(Guid id)
        {
            //await ((HelloWorldDockPane)_dockingManager.GetDockPane("HelloWorld_HelloWorldDockPane")).ShowDockPaneAsync().;
        }
    }
}
