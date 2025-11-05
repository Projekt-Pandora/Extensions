using System;
using System.Reflection;

namespace Pandora.Extensions.Mediator.Builder
{
    public interface IMediatorOptionsBuilder
    {
        MediatorOptionsBuilder RegisterHandler(Type type);
        MediatorOptionsBuilder RegisterHandler<T>();
        MediatorOptionsBuilder RegisterHandlersFromAssembly(Assembly assembly);
        MediatorOptionsBuilder RegisterHandlersFromAssembly<T>();
    }
}