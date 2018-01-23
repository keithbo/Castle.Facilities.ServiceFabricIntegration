namespace TestStateless
{
    using Castle.Facilities.ServiceFabricIntegration;
    using Castle.Facilities.TypedFactory;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

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
            container.AddFacility<ServiceFabricFacility>();

            container.Register(
                Component.For<TestStateless>().AsStatelessService("TestStatelessType"));
        }
    }
}