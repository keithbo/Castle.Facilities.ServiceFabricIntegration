namespace Tests.Resolvers
{
    using System;
    using Castle.Facilities.ServiceFabricIntegration.Resolvers;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Moq;
    using ServiceFabric.Mocks;
    using Xunit;

    public class StatefulServiceResolverTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var kernelMock = new Mock<IKernel>();

            new StatefulServiceResolver(kernelMock.Object, typeof(StatefulService));
            new StatefulServiceResolver(kernelMock.Object, typeof(TestStatefulService));
            Assert.Throws<ArgumentException>(() => new StatefulServiceResolver(kernelMock.Object, typeof(object)));
        }

        [Fact]
        public void ResolveTest()
        {
            using (var container = new WindsorContainer())
            {
                var resolver = new StatefulServiceResolver(container.Kernel, typeof(TestStatefulService))
                {
                    
                };

                var context = MockStatefulServiceContextFactory.Default;

                Assert.Throws<ComponentNotFoundException>(() => resolver.Resolve(context));

                container.Register(Component.For<TestStatefulService>().LifestyleTransient());

                var service = resolver.Resolve(context);

                Assert.NotNull(service);
                var testService = Assert.IsType<TestStatefulService>(service);

                Assert.Null(testService.ReliableStateManagerReplica);

                var stateManagerConfig = new ReliableStateManagerConfiguration();
                var stateManagerDependencyType = typeof(IReliableStateManagerReplica);

                resolver = new StatefulServiceResolver(container.Kernel, typeof(TestStatefulService))
                {
                    StateManagerConfiguration = stateManagerConfig,
                    StateManagerDependencyType = stateManagerDependencyType
                };

                service = resolver.Resolve(context);

                Assert.NotNull(service);
                testService = Assert.IsType<TestStatefulService>(service);

                Assert.NotNull(testService.ReliableStateManagerReplica);
            }
        }
    }
}