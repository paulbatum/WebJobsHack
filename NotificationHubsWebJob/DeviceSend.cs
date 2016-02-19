using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using WebJobs.NotificationHubs;

namespace NotificationHubsWebJob
{
    public class DeviceSend
    {
        public static void SendNotification([WebHookTrigger] string message, [NotificationHub] out Notification notification)
        {
            notification = new TemplateNotification(new Dictionary<string, string> { ["message"] = message });
        }

        
        public static void CronJob([TimerTrigger("00:30:00")] TimerInfo timerInfo, [NotificationHub] out Notification notification)
        {
            notification = new TemplateNotification(new Dictionary<string, string> { ["message"] = "foo" });
        }

    }
}
