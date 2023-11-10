using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;
using Prism.Events;
using Prism.Mef.Modularity;
using Prism.Modularity;

namespace Hello_World_Sample
{
    [ModuleExport(typeof(HelloWorldModule), InitializationMode = InitializationMode.WhenAvailable)]
    internal class HelloWorldModule : IModule
    {
        private readonly IEventAggregator _eventAggregator;

        [ImportingConstructor]
        public HelloWorldModule(IEventAggregator evenAggregator)
        {
            _eventAggregator = evenAggregator;
        }
        // Modules will be initialized during startup. Any work that needs to be done at startup can
        // be initiated from here.
        public void Initialize()
        {

        }
    }
}
