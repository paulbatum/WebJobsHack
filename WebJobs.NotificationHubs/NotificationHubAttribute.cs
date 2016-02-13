using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebJobs.NotificationHubs
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class NotificationHubAttribute : Attribute
    {
        public string TagExpression { get; set; }
    }
}
