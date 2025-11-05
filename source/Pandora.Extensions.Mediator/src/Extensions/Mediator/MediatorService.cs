using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Pandora.Extensions.Mediator;

internal sealed class MediatorService : IMediatorService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MediatorService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task RaiseAsync<T>(T notification, CancellationToken ct = default) 
        where T : class
    {
        if (notification is null)
        {
            throw new ArgumentNullException(nameof(notification));
        }

        await using var scope = _scopeFactory.CreateAsyncScope();
        var handlers = scope.ServiceProvider.GetServices<ISignalHandler<T>>();

        List<Exception>? errors = null;

        foreach (var handler in handlers)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                await handler.HandleAsync(notification, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                errors ??= new List<Exception>(4);
                errors.Add(ex);
            }
        }

        if (errors is { Count: > 0 })
            throw new AggregateException(errors);
    }

    public void Raise<T>(T notification) 
        where T : class
    {
        if (notification is null)
        {
            throw new ArgumentNullException(nameof(notification));
        }

        using var scope = _scopeFactory.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<ISignalHandler<T>>();

        List<Exception>? errors = null;

        foreach (var handler in handlers)
        {
            try
            {
                handler.HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                errors ??= new List<Exception>(4);
                errors.Add(ex);
            }
        }

        if (errors is { Count: > 0 })
        {
            throw new AggregateException(errors);
        }
    }
}
