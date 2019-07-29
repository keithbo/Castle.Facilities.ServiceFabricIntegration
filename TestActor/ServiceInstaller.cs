namespace TestActor
{
    using Castle.Facilities.ServiceFabricIntegration;
    using Castle.Facilities.TypedFactory;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;
    using Microsoft.ServiceFabric.Actors.Runtime;

    /// <summary>
    /// Default windsor installer for basic test setup
    /// </summary>
    public class ServiceInstaller : IWindsorInstaller
    {
        /// <summary>
        /// Performs the installation in the <see cref="T:Castle.Windsor.IWindsorContainer" />.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="store">The configuration store.</param>
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.AddFacility<TypedFactoryFacility>();
            container.AddFacility<ServiceFabricFacility>(f => f.Configure(c => c.UsingActors()));

            container.Register(
                Component.For<TestActorService>().LifestyleTransient(),
                Component.For<TestActor>().AsActor(c => c.ServiceSettings = new ActorServiceSettings
                {
                    ActorConcurrencySettings = new ActorConcurrencySettings
                    {
                        ReentrancyMode = ActorReentrancyMode.Disallowed
                    }
                }).LifestyleTransient()
            );
        }
    }
}