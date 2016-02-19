using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WebJobs.NotificationHubs
{
    internal class NotificationHubAttributeBindingProvider : IBindingProvider
    {
        private readonly NotificationHubConfiguration _config;
        private readonly INameResolver _nameResolver;

        public NotificationHubAttributeBindingProvider(NotificationHubConfiguration config, INameResolver nameResolver)
        {
            _config = config;
            _nameResolver = nameResolver;
        }

        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            throw new NotImplementedException();

            ParameterInfo parameter = context.Parameter;
            NotificationHubAttribute attribute = parameter.GetCustomAttribute<NotificationHubAttribute>(inherit: false);
            if (attribute == null)
            {
                return Task.FromResult<IBinding>(null);
            }

            if (context.Parameter.ParameterType != typeof(TemplateNotification) &&
                context.Parameter.ParameterType != typeof(TemplateNotification).MakeByRefType())
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                    "Can't bind SendGridAttribute to type '{0}'.", parameter.ParameterType));
            }

            if (string.IsNullOrEmpty(_config.ConnectionString))
            {
                throw new InvalidOperationException(
                    string.Format("The Notification Hub connection string must be set either via a '{0}' app setting, via a '{0}' environment variable, or directly in code via NotificationHubConfiguration.ConnectionString.",
                    NotificationHubConfiguration.NotificationHubConnectionStringName));
            }

            if (string.IsNullOrEmpty(_config.HubName))
            {
                throw new InvalidOperationException(
                    string.Format("The Notification Hub name must be set either via a '{0}' app setting, via a '{0}' environment variable, or directly in code via NotificationHubConfiguration.HubName.",
                    NotificationHubConfiguration.NotificationHubSettingName));
            }

            //return Task.FromResult<IBinding>(new NotificationHubBinding(parameter, attribute, _config, _nameResolver, context));
            return Task.FromResult<IBinding>(null);
        }
    }
}
