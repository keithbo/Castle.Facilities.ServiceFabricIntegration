namespace Castle.Facilities.ServiceFabricIntegration
{
    using Castle.Core;
    using Castle.MicroKernel;

    /// <summary>
    /// Abstraction interface for <see cref="ServiceFabricFacility"/> to allow non-default implementations to participate
    /// without needing to change the base implementation.
    /// </summary>
    public interface IServiceFabricModule
    {
        /// <summary>
        /// Called during the <see cref="ComponentModel"/> contribution phase of container configuration.
        /// </summary>
        /// <param name="kernel"><see cref="IKernel"/></param>
        /// <param name="model"><see cref="ComponentModel"/></param>
        void Contribute(IKernel kernel, ComponentModel model);

        /// <summary>
        /// Tests an <see cref="IHandler"/> to see if it should be passed to <see cref="RegisterComponent"/>
        /// </summary>
        /// <param name="handler"><see cref="IHandler"/></param>
        /// <returns></returns>
        bool CanRegister(IHandler handler);

        /// <summary>
        /// Perform all registrations necessary for the provided <see cref="IHandler"/>
        /// </summary>
        /// <param name="kernel"><see cref="IKernel"/></param>
        /// <param name="handler"><see cref="IHandler"/></param>
        void RegisterComponent(IKernel kernel, IHandler handler);
    }
}
