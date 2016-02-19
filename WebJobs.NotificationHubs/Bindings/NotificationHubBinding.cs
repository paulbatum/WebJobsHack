using Microsoft.Azure.WebJobs.Host.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Protocols;

namespace WebJobs.NotificationHubs
{
    internal class NotificationHubBinding : IBinding
    {
        
        public bool FromAttribute
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Task<IValueProvider> BindAsync(BindingContext context)
        {
            throw new NotImplementedException();
        }

        public Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
        {
            throw new NotImplementedException();
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            throw new NotImplementedException();
        }
    }
}
