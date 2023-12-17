using System.ComponentModel.Composition;
using Prism.Mef.Modularity;
using Prism.Modularity;

namespace Hello_World_Sample
{
    [ModuleExport(typeof(HelloWorldModule), InitializationMode = InitializationMode.WhenAvailable)]
    /* If you have services in your plugin that need to be initialized at startup, it's best to start those services from a Module
     * */
    internal class HelloWorldModule : IModule
    {
        [ImportingConstructor]
        public HelloWorldModule() { }
        // Modules will be initialized during startup. Any work that needs to be done at startup can
        // be initiated from here.
        public void Initialize() { }
    }
}
