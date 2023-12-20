using Hello_World_Sample.Common;

using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            BitmapImage bitmapImage = new BitmapImage();
            
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri("pack://application:,,,/Hello World Sample;component/assets/hw_notification_icon.png");
            bitmapImage.EndInit();

            return bitmapImage;
        }
    }
}