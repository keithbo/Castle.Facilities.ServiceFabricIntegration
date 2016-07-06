namespace Castle.Facilities.ServiceFabricIntegration
{
    using Castle.MicroKernel.Registration;
    using Microsoft.ServiceFabric.Actors.Runtime;

    /// <summary>
    /// Fluent API registration extension methods.
    /// </summary>
    public static class ActorRegistrationExtensions
    {
        /// <summary>
        /// Registers the generic argument type <typeparamref name="TActor"/> as an actor type.
        /// </summary>
        /// <typeparam name="TActor">Type must derive from ActorBase</typeparam>
        /// <param name="registration">ComponentRegistration instance</param>
        /// <returns>ComponentRegistration instance for continued registration</returns>
        public static ComponentRegistration<TActor> AsActor<TActor>(this ComponentRegistration<TActor> registration)
            where TActor : ActorBase
        {
            return registration.AddAttributeDescriptor(FacilityConstants.ActorKey, bool.TrueString);
        }

        /// <summary>
        /// Configure the <see cref="ServiceFabricFacility"/> to support Actor registration.
        /// </summary>
        /// <param name="configurer"><see cref="IServiceFabricFacilityConfigurer"/></param>
        /// <returns><see cref="IServiceFabricFacilityConfigurer"/></returns>
        public static IServiceFabricFacilityConfigurer UsingActors(this IServiceFabricFacilityConfigurer configurer)
        {
            return configurer.Using(new ActorModule());
        }
    }
}
