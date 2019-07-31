namespace Tests.Resolvers
{
    using System;
    using Castle.Facilities.ServiceFabricIntegration.Resolvers;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Moq;
    using ServiceFabric.Mocks;
    using Xunit;

    public class ActorServiceResolverTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var kernelMock = new Mock<IKernel>();

            new ActorServiceResolver(kernelMock.Object, typeof(ActorService));
            new ActorServiceResolver(kernelMock.Object, typeof(TestActorService));
            Assert.Throws<ArgumentException>(() => new ActorServiceResolver(kernelMock.Object, typeof(object)));
        }

        [Fact]
        public void ResolveTest()
        {
            using (var container = new WindsorContainer())
            {
                var resolver = new ActorServiceResolver(container.Kernel, typeof(TestActorService))
                {

                };

                var context = MockStatefulServiceContextFactory.Default;
                var typeInfo = ActorTypeInformation.Get(typeof(TestActor));

                Assert.Throws<ComponentNotFoundException>(() => resolver.Resolve(context, typeInfo));

                container.Register(Component.For<TestActorService>().LifestyleTransient());

                var service = resolver.Resolve(context, typeInfo);

                Assert.NotNull(service);
                var testService = Assert.IsType<TestActorService>(service);

                Assert.Null(testService.StateManagerFactory);
                Assert.Null(testService.ActorFactory);
                Assert.Null(testService.StateProvider);
                Assert.Null(testService.Settings);
                Assert.Same(typeInfo, testService.ActorTypeInfo);
                Assert.Same(context, testService.Context);

                // ReSharper disable once ConvertToLocalFunction -- Local function doesn't equal a realized Func<>
                Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactoryMock =
                    (actor, provider) => new MockActorStateManager();

                // ReSharper disable once ConvertToLocalFunction -- Local function doesn't equal a realized Func<>
                Func<ActorService, ActorId, ActorBase> actorFactoryMock =
                    (s, id) => new TestActor(s, id);

                var stateProviderMock = new Mock<IActorStateProvider>();

                var settings = new ActorServiceSettings();

                resolver = new ActorServiceResolver(container.Kernel, typeof(TestActorService))
                {
                    StateManagerFactory = stateManagerFactoryMock,
                    ActorFactory = actorFactoryMock,
                    StateProvider = stateProviderMock.Object,
                    Settings = settings
                };

                service = resolver.Resolve(context, typeInfo);

                Assert.NotNull(service);
                testService = Assert.IsType<TestActorService>(service);

                Assert.Same(stateManagerFactoryMock, testService.StateManagerFactory);
                Assert.Same(actorFactoryMock, testService.ActorFactory);
                Assert.Same(stateProviderMock.Object, testService.StateProvider);
                Assert.Same(settings, testService.Settings);
                Assert.Same(typeInfo, testService.ActorTypeInfo);
                Assert.Same(context, testService.Context);
            }
        }
    }
}