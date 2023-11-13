using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;
using WinTak.Framework.Docking;
using WinTak.Framework.Tools;
using WinTak.Framework.Tools.Attributes;

namespace Hello_World_Sample
{
    [Button("HelloWorld_HelloWorldButton", "HelloWorld Plugin",
    LargeImage = "pack://application:,,,/Hello World Sample;component/assets/ic_launcher.svg",
    SmallImage = "pack://application:,,,/Hello World Sample;component/assets/ic_launcher_24x24.png")]
    internal class HelloWorldButton : Button
    {
        private IDockingManager _dockingManager;

        [ImportingConstructor]
        public HelloWorldButton(IDockingManager dockingManager)
        {
            _dockingManager = dockingManager;
        }

        protected override void OnClick()
        {
            base.OnClick();

            var pane = _dockingManager.GetDockPane(HelloWorldDockPane.ID);
            pane?.Activate();
        }


    }
}
