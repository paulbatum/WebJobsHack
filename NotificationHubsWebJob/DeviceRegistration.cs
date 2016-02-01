using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationHubsWebJob
{
    public class DeviceRegistration
    {
        
        public static async Task Register([WebHookTrigger] Installation installation, TraceWriter trace)
        {
            var nhConnectionString = ConfigurationManager.ConnectionStrings["MS_NotificationHubConnectionString"].ConnectionString;

            // Create the notification hub client.
            NotificationHubClient hub = NotificationHubClient
                .CreateClientFromConnectionString(nhConnectionString, "pbjobs");

            await hub.CreateOrUpdateInstallationAsync(installation);
            trace.Info("Registration created");
        }
    }
}
