using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Extensions.Mediator
{
    public class MediatorRegisterUtility
    {
        public static void RegisterHandlers(IServiceCollection services, IEnumerable<Type> types)
        {
            var handlerTypes = from type in types
                               from serviceType in (type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISignalHandler<>)))
                               select new { ServiceType = serviceType, ImpType = type };

            foreach (var handlerType in handlerTypes)
            {
                services.TryAddTransient(handlerType.ServiceType, handlerType.ImpType);
            }
        }
    }
}
