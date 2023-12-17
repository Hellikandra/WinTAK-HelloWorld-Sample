using Hello_World_Sample.Common;
using System.ComponentModel.Composition;
using System.Windows.Media;
using WinTak.Framework.Notifications;


namespace Hello_World_Sample.Notifications
{
    [Export(typeof(Notification))]
    public class HelloWorldNotification : Notification
    {
        public HelloWorldNotification() 
        {
            base.Type = ULIDGenerator.GenerateULID();
            this.Key = ULIDGenerator.GenerateULID();
        }
        public override string GetHeader()
        {
            return "HelloWorldNotification";
        }
        public override ImageSource GetHeaderIcon()
        {
            return null;
        }
    }
}