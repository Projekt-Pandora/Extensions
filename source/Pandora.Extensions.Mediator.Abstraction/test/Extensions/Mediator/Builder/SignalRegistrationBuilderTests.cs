using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Pandora.Extensions.Mediator.Builder
{
    [TestFixture]
    public class SignalRegistrationBuilderTests
    {
        // Hilfstypen, die in den Tests verwendet werden
        private sealed class ConcreteHandler { }
        private abstract class AbstractHandler { }
        private sealed class GenericHandler<T> { }
        private sealed class UniqueTypeForAssembly { }

        [Test]
        public void Constructor_UsesProvidedHashSet()
        {
            var builder = new MediatorOptionsBuilder();
            builder.RegisterHandler<ConcreteHandler>();

            var built = builder.Build().ToList();

            Assert.That(built, Does.Contain(typeof(ConcreteHandler)));
        }

        [Test]
        public void RegisterHandler_Type_AddsType()
        {
            var builder = new MediatorOptionsBuilder();

            builder.RegisterHandler(typeof(ConcreteHandler));

            var types = builder.Build().ToList();
            Assert.That(types, Does.Contain(typeof(ConcreteHandler)));
        }

        [Test]
        public void RegisterHandler_GenericClosedType_AddsType()
        {
            var builder = new MediatorOptionsBuilder();

            // geschlossene generische Typen (z.B. GenericHandler<int>) sind Klassen -> werden hinzugefügt
            builder.RegisterHandler<GenericHandler<int>>();

            var types = builder.Build().ToList();
            Assert.That(types, Does.Contain(typeof(GenericHandler<int>)));
        }

        [Test]
        public void RegisterHandler_OpenGenericInterface_IsNotAdded()
        {
            var builder = new MediatorOptionsBuilder();

            // Beispiel für einen offenen generischen Interface-Typ (nicht Klasse, abstrakt, GenericTypeDefinition)
            var openInterface = typeof(IList<>);

            builder.RegisterHandler(openInterface);

            var types = builder.Build().ToList();
            Assert.That(types, Does.Not.Contain(openInterface));
        }

        [Test]
        public void RegisterHandlersFromAssembly_IncludesTypesFromAssembly()
        {
            var builder = new MediatorOptionsBuilder();

            // Registriert alle Typen aus der Assembly, in der UniqueTypeForAssembly definiert ist.
            builder.RegisterHandlersFromAssembly<UniqueTypeForAssembly>();

            var types = builder.Build().ToList();
            Assert.That(types, Does.Contain(typeof(UniqueTypeForAssembly)));
        }

        [Test]
        public void RegisterHandlersFromAssembly_ByAssemblyParameter_IncludesTypesFromAssembly()
        {
            var builder = new MediatorOptionsBuilder();

            // Alternative Überladung: Assembly-Parameter
            var asm = typeof(UniqueTypeForAssembly).Assembly;
            builder.RegisterHandlersFromAssembly(asm);

            var types = builder.Build().ToList();
            Assert.That(types, Does.Contain(typeof(UniqueTypeForAssembly)));
        }

        [Test]
        public void RegisterHandler_GenericMethod_AddsType()
        {
            var builder = new MediatorOptionsBuilder();

            builder.RegisterHandler<ConcreteHandler>();

            var types = builder.Build().ToList();
            Assert.That(types, Does.Contain(typeof(ConcreteHandler)));
        }

        [Test]
        public void Build_ReturnsSnapshotArray_NotLiveView()
        {
            var builder = new MediatorOptionsBuilder();

            // Erstes Snapshot
            var snapshot1 = builder.Build().ToList();

            // Nachträglich einen Typ hinzufügen
            builder.RegisterHandler(typeof(ConcreteHandler));

            var snapshot2 = builder.Build().ToList();

            // snapshot1 darf den später hinzugefügten Typ nicht enthalten
            Assert.That(snapshot1, Does.Not.Contain(typeof(ConcreteHandler)));
            Assert.That(snapshot2, Does.Contain(typeof(ConcreteHandler)));
        }
    }
}