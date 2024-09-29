using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinTak.Common.Geofence;
using Hello_World_Sample.Geofences;

namespace Hello_World_Sample.Services
{
    internal interface IHelloWorldServices
    {
        GeofencesMonitoring geofencesMonitoring { get; set; }
        void Dispose();
    }
}
