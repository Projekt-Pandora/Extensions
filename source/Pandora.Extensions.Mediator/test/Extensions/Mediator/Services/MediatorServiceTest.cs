using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Pandora.Extensions.Mediator.Services
{
    // Einfacher DTO/Signal für Tests
    public sealed class TestNotification { public string? Message { get; init; } }

    [TestFixture]
    public class MediatorServiceTest
    {
        // Hilfsklasse: synchroner IServiceScope
        private sealed class SyncServiceScope : IServiceScope
        {
            public IServiceProvider ServiceProvider { get; }
            public SyncServiceScope(IServiceProvider provider) => ServiceProvider = provider;
            public void Dispose() { /* no-op */ }
        }

        // Hilfsklasse: IAsyncServiceScope (IServiceScope + IAsyncDisposable)
        private sealed class TestAsyncServiceScope : IServiceScope, IAsyncDisposable
        {
            public IServiceProvider ServiceProvider { get; }
            public TestAsyncServiceScope(IServiceProvider provider) => ServiceProvider = provider;
            public void Dispose() { /* no-op */ }
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }

        [Test]
        public void Raise_NullNotification_Throws()
        {
            var factory = new Mock<IServiceScopeFactory>();
            var svc = new MediatorService(factory.Object);

            Assert.Throws<ArgumentNullException>(() => svc.Raise<TestNotification>(null!));
        }

        [Test]
        public async Task RaiseAsync_NullNotification_Throws()
        {
            var factory = new Mock<IServiceScopeFactory>();
            var svc = new MediatorService(factory.Object);

            Assert.ThrowsAsync<ArgumentNullException>(async () => await svc.RaiseAsync<TestNotification>(null!));
        }

        [Test]
        public async Task RaiseAsync_InvokesHandlers_AndRespectsCancellation()
        {
            var cts = new CancellationTokenSource();

            var handler1 = new Mock<ISignalHandler<TestNotification>>();
            handler1
                .Setup(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
                .Returns<TestNotification, CancellationToken>(async (n, token) =>
                {
                    // Erstes Handler ruft Cancel, so dass der zweite Handler nicht mehr ausgeführt wird
                    cts.Cancel();
                    await Task.Delay(1, CancellationToken.None);
                });

            var handler2 = new Mock<ISignalHandler<TestNotification>>();
            handler2
                .Setup(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // DI-Container mit den Handlern
            var services = new ServiceCollection();
            services.AddSingleton(handler1.Object);
            services.AddSingleton(handler2.Object);
            var provider = services.BuildServiceProvider();

            var factory = new Mock<IServiceScopeFactory>();
            // Nicht die Erweiterungsmethode (CreateAsyncScope) mocken — stattdessen CreateScope bereitstellen.
            factory.Setup(f => f.CreateScope()).Returns(() => provider.CreateScope());

            var svc = new MediatorService(factory.Object);

            // Übergabe des CancellationToken aus dem cts; erstes Handler cancelt, zweites darf nicht aufgerufen werden
            await svc.RaiseAsync(new TestNotification { Message = "hi" }, cts.Token);

            handler1.Verify(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()), Times.Once);
            // handler2 darf nicht gerufen werden, weil nach dem ersten Handler der Token gesetzt wurde und Service abbricht
            handler2.Verify(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task RaiseAsync_CollectsExceptions_AndThrowsAggregateException()
        {
            var handler1 = new Mock<ISignalHandler<TestNotification>>();
            handler1
                .Setup(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("h1"));

            var handler2 = new Mock<ISignalHandler<TestNotification>>();
            handler2
                .Setup(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ApplicationException("h2"));

            var services = new ServiceCollection();
            services.AddSingleton(handler1.Object);
            services.AddSingleton(handler2.Object);
            var provider = services.BuildServiceProvider();

            var factory = new Mock<IServiceScopeFactory>();
            // Nicht die Erweiterungsmethode (CreateAsyncScope) mocken — stattdessen CreateScope bereitstellen.
            factory.Setup(f => f.CreateScope()).Returns(() => provider.CreateScope());

            var svc = new MediatorService(factory.Object);

            var ex = Assert.ThrowsAsync<AggregateException>(async () => await svc.RaiseAsync(new TestNotification()));
            Assert.That(ex!.InnerExceptions, Has.Exactly(2).Items);
        }

        [Test]
        public void Raise_Sync_CollectsExceptions_AndThrowsAggregateException()
        {
            var handler1 = new Mock<ISignalHandler<TestNotification>>();
            handler1
                .Setup(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("h1"));

            var handler2 = new Mock<ISignalHandler<TestNotification>>();
            handler2
                .Setup(h => h.HandleAsync(It.IsAny<TestNotification>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ApplicationException("h2"));

            var services = new ServiceCollection();
            services.AddSingleton(handler1.Object);
            services.AddSingleton(handler2.Object);
            var provider = services.BuildServiceProvider();

            var factory = new Mock<IServiceScopeFactory>();
            // Nur CreateScope bereitstellen; CreateAsyncScope wird als Erweiterung über CreateScope funktionieren.
            factory.Setup(f => f.CreateScope()).Returns(() => provider.CreateScope());

            var svc = new MediatorService(factory.Object);

            var ex = Assert.Throws<AggregateException>(() => svc.Raise(new TestNotification()));
            Assert.That(ex!.InnerExceptions, Has.Exactly(2).Items);
        }
    }
}