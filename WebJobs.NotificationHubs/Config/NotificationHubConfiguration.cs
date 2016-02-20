using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.ServiceBus;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebJobs.NotificationHubs
{
    public class NotificationHubConfiguration : IExtensionConfigProvider
    {
        internal const string NotificationHubConnectionStringName = "MS_NotificationHubConnectionString";
        internal const string NotificationHubSettingName = "MS_NotificationHubName";

        public NotificationHubConfiguration()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings[NotificationHubConnectionStringName]?.ConnectionString;
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

        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var converterManager = context.Config.GetOrCreateConverterManager();
            converterManager.AddConverter<TemplateNotification, Notification>(x => x);
            
            var provider = new NotificationHubAttributeBindingProvider(context.Config.NameResolver, converterManager, this);
            context.Config.RegisterBindingExtension(provider);
        }
    }
}
