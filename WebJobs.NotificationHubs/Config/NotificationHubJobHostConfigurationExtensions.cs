using Microsoft.Azure.WebJobs;
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

            config.RegisterExtensionConfigProvider(new NotificationHubExtensionConfig(notificationHubConfig));

        }

        private class NotificationHubExtensionConfig : IExtensionConfigProvider
        {
            private readonly NotificationHubConfiguration _notificationHubConfig;

            public NotificationHubExtensionConfig(NotificationHubConfiguration notificationHubConfig)
            {
                _notificationHubConfig = notificationHubConfig;
            }

            public void Initialize(ExtensionConfigContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                context.Config.RegisterBindingExtension(new NotificationHubAttributeBindingProvider(context.Config.NameResolver, _notificationHubConfig));
            }
        }

    }
}
