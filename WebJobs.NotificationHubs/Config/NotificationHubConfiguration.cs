using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebJobs.NotificationHubs
{
    public class NotificationHubConfiguration
    {
        internal const string NotificationHubConnectionStringName = "MS_NotificationHubConnectionString";
        internal const string NotificationHubSettingName = "MS_NotificationHubName";

        public NotificationHubConfiguration()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings[NotificationHubConnectionStringName].ConnectionString;
            if(string.IsNullOrEmpty(ConnectionString))
            {
                ConnectionString = Environment.GetEnvironmentVariable(NotificationHubConnectionStringName);
            }

            HubName = ConfigurationManager.AppSettings[NotificationHubSettingName];
            if (string.IsNullOrEmpty(HubName))
            {
                HubName = Environment.GetEnvironmentVariable(NotificationHubSettingName);
            }
        }

        public string ConnectionString { get; set; }

        public string HubName { get; set; }
    }
}
