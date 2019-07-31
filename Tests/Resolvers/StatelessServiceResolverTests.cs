namespace Tests.Resolvers
{
    using System;
    using Castle.Facilities.ServiceFabricIntegration.Resolvers;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Moq;
    using ServiceFabric.Mocks;
    using Xunit;

    public class StatelessServiceResolverTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var kernelMock = new Mock<IKernel>();

            new StatelessServiceResolver(kernelMock.Object, typeof(StatelessService));
            new StatelessServiceResolver(kernelMock.Object, typeof(TestStatelessService));
            Assert.Throws<ArgumentException>(() => new StatefulServiceResolver(kernelMock.Object, typeof(object)));
        }

        [Fact]
        public void ResolveTest()
        {
            using (var container = new WindsorContainer())
            {
                var resolver = new StatelessServiceResolver(container.Kernel, typeof(TestStatelessService))
                {

                };

                var context = MockStatelessServiceContextFactory.Default;

                Assert.Throws<ComponentNotFoundException>(() => resolver.Resolve(context));

                container.Register(Component.For<TestStatelessService>().LifestyleTransient());

                var service = resolver.Resolve(context);

                Assert.NotNull(service);
                Assert.IsType<TestStatelessService>(service);

                resolver = new StatelessServiceResolver(container.Kernel, typeof(TestStatelessService));

                service = resolver.Resolve(context);

                Assert.NotNull(service);
                Assert.IsType<TestStatelessService>(service);
            }
        }
    }
}