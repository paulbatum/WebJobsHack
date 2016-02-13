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

namespace NotificationHubsWebJob
{
    public class DeviceSend
    {
        public static async Task SendNotification([WebHookTrigger] string message, TraceWriter trace)
        {
            var nhConnectionString = ConfigurationManager.ConnectionStrings["MS_NotificationHubConnectionString"].ConnectionString;

            // Create the notification hub client.
            NotificationHubClient hub = NotificationHubClient
                .CreateClientFromConnectionString(nhConnectionString, "pbjobs");

            // Define a WNS payload
            var windowsToastPayload = @"<toast><visual><binding template=""ToastText01""><text id=""1"">"
                                    + message + @"</text></binding></visual></toast>";
            try
            {
                // Send the push notification.
                var result = await hub.SendWindowsNativeNotificationAsync(windowsToastPayload);

                // Write the success result to the logs.
                trace.Info(result.State.ToString());
            }
            catch (System.Exception ex)
            {
                // Write the failure result to the logs.
                trace.Error(ex.Message, null, "Push.SendAsync Error");
            }
        }

        public static async Task CronJob([TimerTrigger("*/15 * * * * *")] TimerInfo timerInfo, TraceWriter trace)
        {
            var nhConnectionString = ConfigurationManager.ConnectionStrings["MS_NotificationHubConnectionString"].ConnectionString;

            // Create the notification hub client.
            NotificationHubClient hub = NotificationHubClient
                .CreateClientFromConnectionString(nhConnectionString, "pbjobs");
            
            try
            {
                // Send the push notification.
                //var result = await hub.SendWindowsNativeNotificationAsync(windowsToastPayload);
                var result = await hub.SendTemplateNotificationAsync(new Dictionary<string, string> { ["message"] = "foo" });

                // Write the success result to the logs.
                trace.Info(result.State.ToString());
            }
            catch (System.Exception ex)
            {
                // Write the failure result to the logs.
                trace.Error(ex.Message, null, "Push.SendAsync Error");
            }
        }

    }
}
