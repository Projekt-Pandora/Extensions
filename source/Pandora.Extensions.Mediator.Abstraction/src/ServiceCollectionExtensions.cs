using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pandora.Extensions.Mediator;
using Pandora.Extensions.Mediator.Builder;

namespace Pandora;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureMediator(this IServiceCollection services, Action<IMediatorOptionsBuilder>? configure = null)
    {
        var builder = new MediatorOptionsBuilder();

        configure?.Invoke(builder);

        MediatorRegisterUtility.RegisterHandlers(services, builder.Build());
        
        return services;
    }


}
