namespace Tests.Resolvers
{
    using System;
    using System.ComponentModel;
    using Castle.Facilities.ServiceFabricIntegration.Resolvers;
    using Castle.MicroKernel;
    using Castle.Windsor;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Moq;
    using ServiceFabric.Mocks;
    using Xunit;
    using Component=Castle.MicroKernel.Registration.Component;

    public class ActorResolverTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var kernelMock = new Mock<IKernel>();

            new ActorResolver(kernelMock.Object, typeof(Actor));
            new ActorResolver(kernelMock.Object, typeof(TestActor));
            Assert.Throws<ArgumentException>(() => new ActorResolver(kernelMock.Object, typeof(object)));
        }

        [Fact]
        public void ResolveTest()
        {
            using (var container = new WindsorContainer())
            {
                var resolver = new ActorResolver(container.Kernel, typeof(TestActor));

                var actorService = MockActorServiceFactory.CreateActorServiceForActor<TestActor>();

                Assert.Throws<ComponentNotFoundException>(() => resolver.Resolve(actorService, new ActorId(1)));

                container.Register(
                    Component.For<TestActor>().LifestyleTransient());

                var actor = resolver.Resolve(actorService, new ActorId(1));

                Assert.NotNull(actor);
                var testActor = Assert.IsType<TestActor>(actor);

                Assert.Equal(new ActorId(1), testActor.ActorId);
            }
        }
    }
}