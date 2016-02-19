using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.ServiceBus;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace WebJobs.NotificationHubs
{
    internal class NotificationHubAttributeBindingProvider : IBindingProvider
    {
        private readonly NotificationHubConfiguration _notificationHubConfig;
        private readonly INameResolver _nameResolver;

        public NotificationHubAttributeBindingProvider(INameResolver nameResolver, NotificationHubConfiguration config)
        {            
            _nameResolver = nameResolver;
            _notificationHubConfig = config;
        }

        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }            

            ParameterInfo parameter = context.Parameter;
            NotificationHubAttribute attribute = parameter.GetCustomAttribute<NotificationHubAttribute>(inherit: false);
            if (attribute == null)
            {
                return Task.FromResult<IBinding>(null);
            }

            if (context.Parameter.ParameterType != typeof(Notification) &&
                context.Parameter.ParameterType != typeof(Notification).MakeByRefType())
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                    "Can't bind NotificationHubAttribute to type '{0}'.", parameter.ParameterType));
            }

            if (string.IsNullOrEmpty(_notificationHubConfig.ConnectionString))
            {
                throw new InvalidOperationException(
                    string.Format("The Notification Hub connection string must be set either via a '{0}' app setting, via a '{0}' environment variable, or directly in code via NotificationHubConfiguration.ConnectionString.",
                    NotificationHubConfiguration.NotificationHubConnectionStringName));
            }

            if (string.IsNullOrEmpty(_notificationHubConfig.HubName))
            {
                throw new InvalidOperationException(
                    string.Format("The Notification Hub name must be set either via a '{0}' app setting, via a '{0}' environment variable, or directly in code via NotificationHubConfiguration.HubName.",
                    NotificationHubConfiguration.NotificationHubSettingName));
            }

            var notificationHubClient = NotificationHubClient.CreateClientFromConnectionString(
                _notificationHubConfig.ConnectionString, _notificationHubConfig.HubName);

            //return Task.FromResult<IBinding>(new NotificationHubBinding(parameter, attribute, _config, _nameResolver, context));
            IBinding b = GenericBinder.BindCollector<Notification, NotificationHubClient>(
                parameter,
                new NotificationHubConverterManager(),
                notificationHubClient,
                (t, c) => new NotificationHubAsyncCollector(t, attribute.TagExpression),
                "",
                (s) => null);

            return Task.FromResult(b);
        }

        private class NotificationHubConverterManager : IConverterManager
        {
            public Func<TSrc, TDest> GetConverter<TSrc, TDest>()
            {
                return new Func<object, object>(GetConverter) as Func<TSrc, TDest>;
            }

            private object GetConverter(object source)
            {
                return source;
            }

            public void AddConverter<TSrc, TDest>(Func<TSrc, TDest> converter)
            {
            }
        }

        private class NotificationHubAsyncCollector : IFlushCollector<Notification>
        {
            NotificationHubClient _hubClient;
            string _tagExpression;

            public NotificationHubAsyncCollector(NotificationHubClient hubClient, string tagExpression)
            {
                _hubClient = hubClient;
                _tagExpression = tagExpression;
            }

            public async Task AddAsync(Notification item, CancellationToken cancellationToken = default(CancellationToken))
            {                                
                await _hubClient.SendNotificationAsync(item, _tagExpression);
            }

            public Task FlushAsync()
            {
                return Task.FromResult(0);
            }
        }
    }
}
