namespace Tests.Resolvers
{
    using System;
    using Castle.Facilities.ServiceFabricIntegration.Resolvers;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Xunit;

    public class RuntimeRegistrationTests
    {
        [Fact]
        public void StaticInitializationTest()
        {
            Assert.NotNull(RuntimeRegistration.RuntimeRegistrationMethod);
        }

        [Fact]
        public void ConstructorTest()
        {
            new RuntimeRegistration(typeof(TestActor));
            Assert.Throws<ArgumentException>(() => new RuntimeRegistration(typeof(object)));
            Assert.Throws<ArgumentException>(() => new RuntimeRegistration(typeof(ActorBase)));
            Assert.Throws<ArgumentException>(() => new RuntimeRegistration(typeof(Actor)));
        }

        [Fact]
        public void RegistrationMethodExistsTest()
        {
            var registration = new RuntimeRegistration(typeof(TestActor));

            Assert.True(RuntimeRegistration.RegistrationCache.TryGetValue(typeof(TestActor), out var registrationFunc));
            Assert.NotNull(registrationFunc);
        }
    }
}