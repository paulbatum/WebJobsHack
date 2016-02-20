using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebJobs.NotificationHubs
{
    public static class NotificationHubJobHostConfigurationExtensions
    {
        public static void UseNotificationHubs(this JobHostConfiguration config, NotificationHubConfiguration notificationHubConfig = null)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (notificationHubConfig == null)
            {
                notificationHubConfig = new NotificationHubConfiguration();
            }

            
            config.RegisterExtensionConfigProvider(notificationHubConfig);

        }
    }
}
