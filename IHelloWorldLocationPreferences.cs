using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace Hello_World_Sample
{
    internal interface IHelloWorldLocationPreferences : INotifyPropertyChanged
    {
        string StartingLocation { get; set; }
        double Speed { get; set; }
        double Bearing { get; set; }
        int UpdateRate { get; set; }

        [SpecialName]
        [CompilerGenerated]
        void add_PropertyChanged(EventHandler<string> value);

        [SpecialName]
        [CompilerGenerated]
        void remove_PropertyChanged(EventHandler<string> value);
    }
}
