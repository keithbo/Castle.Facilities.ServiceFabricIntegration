namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;
    using Castle.MicroKernel.Registration;
    using Microsoft.ServiceFabric.Actors.Runtime;

    /// <summary>
    /// Fluent API registration extension methods.
    /// </summary>
    public static class ActorRegistrationExtensions
    {
        /// <summary>
        /// Registers the generic argument type <typeparamref name="TActor"/> as an actor type with a default ActorService host.
        /// </summary>
        /// <typeparam name="TActor">Type must derive from ActorBase</typeparam>
        /// <param name="registration">ComponentRegistration instance</param>
        /// <param name="configure">Delegate action to configure actor registration</param>
        /// <returns>ComponentRegistration instance for continued registration</returns>
        public static ComponentRegistration<TActor> AsActor<TActor>(this ComponentRegistration<TActor> registration, Action<IActorConfigurer> configure = null)
            where TActor : ActorBase
        {
            var configuration = new ActorConfiguration<TActor>(registration);
            configure?.Invoke(configuration);

            return configuration.Build();
        }

        /// <summary>
        /// Registers the generic argument type <typeparamref name="TActor"/> as an actor type with an ActorService host of type <typeparamref name="TActorService"/>.
        /// </summary>
        /// <typeparam name="TActor">Type must derive from ActorBase</typeparam>
        /// <typeparam name="TActorService">Type must derive from ActorService</typeparam>
        /// <param name="registration">ComponentRegistration instance</param>
        /// <returns>ComponentRegistration instance for continued registration</returns>
        public static ComponentRegistration<TActor> AsActor<TActor, TActorService>(this ComponentRegistration<TActor> registration)
            where TActor : ActorBase
            where TActorService : ActorService
        {
            return registration.AsActor<TActor>(c => c.WithService<TActorService>());
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

        public static IActorConfigurer WithService<TService>(this IActorConfigurer configurer)
            where TService : ActorService
        {
            var serviceType = typeof(TService);
            configurer.ServiceType = serviceType;
            return configurer;
        }

        public static IActorConfigurer WithService(this IActorConfigurer configurer, Type serviceType)
        {
            ActorHelpers.ValidateServiceType(serviceType);
            configurer.ServiceType = serviceType;
            return configurer;
        }
    }
}
