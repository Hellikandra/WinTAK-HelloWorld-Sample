using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;
using System.Windows.Input;
using WinTak.Framework.Docking;
using WinTak.Framework.Docking.Attributes;

namespace Hello_World_Sample
{
    [DockPane(ID, "HelloWorld", Content = typeof(HelloWorldView))]
    internal class HelloWorldDockPane : DockPane
    {
        internal const string ID = "HelloWorld_HelloWorldDockPane";
        public HelloWorldDockPane()
        {
        }
    }
}
