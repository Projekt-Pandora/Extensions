using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pandora.Extensions.Mediator.Builder;

public sealed class MediatorOptionsBuilder : IMediatorOptionsBuilder
{
    private readonly HashSet<Type> _types = new();

    public MediatorOptionsBuilder()
    { }

    public MediatorOptionsBuilder RegisterHandlersFromAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            RegisterHandler(type);
        }

        return this;
    }

    public MediatorOptionsBuilder RegisterHandlersFromAssembly<T>()
    {
        RegisterHandlersFromAssembly(typeof(T).Assembly);

        return this;
    }

    public MediatorOptionsBuilder RegisterHandler(Type type)
    {
        if (type.IsClass || !type.IsAbstract || !type.IsGenericTypeDefinition)
        {
            _types.Add(type);
        }

        return this;
    }

    public MediatorOptionsBuilder RegisterHandler<T>()
    {
        RegisterHandler(typeof(T));

        return this;
    }

    public IEnumerable<Type> Build()
    {
        return _types.AsEnumerable();
    }
}
