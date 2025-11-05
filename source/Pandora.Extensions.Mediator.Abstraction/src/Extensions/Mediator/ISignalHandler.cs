using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Extensions.Mediator
{
    public interface ISignalHandler<T>
        where T : class
    {
        Task HandleAsync(T signal, CancellationToken ct);
    }
}
