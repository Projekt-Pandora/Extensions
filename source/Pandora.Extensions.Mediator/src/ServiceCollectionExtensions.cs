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
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<IMediatorOptionsBuilder>? configure = null)
    {
        services.TryAddSingleton<IMediatorService, MediatorService>();

        if (configure is not null)
        {
            services.ConfigureMediator(configure);
        }

        return services;
    }
}
