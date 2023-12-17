using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using WinTak.Framework.Preferences;
using WinTak.Framework.Preferences.Attributes;

namespace Hello_World_Sample.Preferences
{

    [Export(typeof(IToolPreference))]
    internal class HelloWorldPreferences : Preference, IToolPreference, IPreference
    {
        private const string soundNotification = "playSound";
        private bool notificationSoundBool;

        [PropertyOrder(1)]
        [SettingsKey("ChangeNotification")]

        public bool PlayNotificationSound
        {
            get
            {
                return this.notificationSoundBool;
            }
            set
            {
                base.SetProperty(ref this.notificationSoundBool, value, "");
            }
        }
    }
}
