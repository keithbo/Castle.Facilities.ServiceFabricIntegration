namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;
    using Castle.MicroKernel.Registration;
    using Microsoft.ServiceFabric.Services.Runtime;

    /// <summary>
    /// Registration extensions to allow Fluent registration api extension during Castle Windsor registration.
    /// </summary>
    public static class ServiceRegistrationExtensions
    {
        /// <summary>
        /// Register the specified service type as a stateless service. A valid service type must be provided.
        /// </summary>
        /// <typeparam name="TService">Type must derive from <see cref="StatelessService"/></typeparam>
        /// <param name="registration">ComponentRegistration instance</param>
        /// <param name="serviceTypeName">ServiceTypeName for the given service class</param>
        /// <returns>ComponentRegistration for continued Fluent API registrations.</returns>
        public static ComponentRegistration<TService> AsStatelessService<TService>(this ComponentRegistration<TService> registration, string serviceTypeName)
            where TService : StatelessService
        {
            var configuration = new StatelessConfiguration<TService>(registration)
            {
                ServiceTypeName = serviceTypeName
            };

            return configuration.Build();
        }

        /// <summary>
        /// Register the specified service type as a stateful service. A valid service type must be provided.
        /// </summary>
        /// <typeparam name="TService">Type must derive from <see cref="StatefulServiceBase"/></typeparam>
        /// <param name="registration">ComponentRegistration instance</param>
        /// <param name="serviceTypeName">ServiceTypeName for the given service class</param>
        /// <param name="configure">Delegate action to configure service registration</param>
        /// <returns>ComponentRegistration for continued Fluent API registrations.</returns>
        public static ComponentRegistration<TService> AsStatefulService<TService>(this ComponentRegistration<TService> registration, string serviceTypeName, Action<IStatefulConfigurer> configure = null)
            where TService : StatefulServiceBase
        {
            var configuration = new StatefulConfiguration<TService>(registration)
            {
                ServiceTypeName = serviceTypeName
            };
            configure?.Invoke(configuration);

            return configuration.Build();
        }
    }
}
