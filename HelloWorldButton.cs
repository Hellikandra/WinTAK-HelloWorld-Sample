using System.ComponentModel.Composition;
using WinTak.Common.Services;
using WinTak.Framework.Docking;
using WinTak.Framework.Tools;
using WinTak.Framework.Tools.Attributes;

namespace Hello_World_Sample
{
    [Button("HelloWorld_HelloWorldButton", "HelloWorld Plugin",
    Tab = "Samples",
    TabGroup = "Hello World",
    LargeImage = "pack://application:,,,/Hello World Sample;component/assets/ic_launcher.svg",
    SmallImage = "pack://application:,,,/Hello World Sample;component/assets/ic_launcher_24x24.png")]
    // Ensure that in the Properties of the image on the right, the Build Action is set to Resource
    /* Buttons are placed on the Ribbon panel of WinTak. Buttons can be used to initiate 
     * map interaction, open dockable windows, or anything else that needs activated.
     * */
    internal class HelloWorldButton : Button
    {
        private readonly IDockingManager _dockingManager;
        private IMapObjectRenderer _renderer;
        [ImportingConstructor]
        public HelloWorldButton(IDockingManager dockingManager, IMapObjectRenderer mapObjectRenderer)
        {
            _dockingManager = dockingManager;
            _renderer = mapObjectRenderer;
        }
        protected override void OnClick()
        {
            base.OnClick();

            var pane = _dockingManager.GetDockPane(HelloWorldDockPane.ID);
            pane?.Activate();
        }
    }
}
