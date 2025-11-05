using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Extensions.Mediator;

public interface IMediatorService
{
    Task RaiseAsync<T>(T notification, CancellationToken ct = default) where T : class;

    void Raise<T>(T notification) where T : class;
}
