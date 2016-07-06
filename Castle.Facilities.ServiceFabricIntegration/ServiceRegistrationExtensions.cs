namespace Castle.Facilities.ServiceFabricIntegration
{
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
            return registration.AddAttributeDescriptor(FacilityConstants.StatelessServiceKey, bool.TrueString)
                               .AddAttributeDescriptor(FacilityConstants.ServiceTypeNameKey, serviceTypeName);
        }

        /// <summary>
        /// Register the specified service type as a stateful service. A valid service type must be provided.
        /// </summary>
        /// <typeparam name="TService">Type must derive from <see cref="StatefulServiceBase"/></typeparam>
        /// <param name="registration">ComponentRegistration instance</param>
        /// <param name="serviceTypeName">ServiceTypeName for the given service class</param>
        /// <returns>ComponentRegistration for continued Fluent API registrations.</returns>
        public static ComponentRegistration<TService> AsStatefulService<TService>(this ComponentRegistration<TService> registration, string serviceTypeName)
            where TService : StatefulServiceBase
        {
            return registration.AddAttributeDescriptor(FacilityConstants.StatefulServiceKey, bool.TrueString)
                               .AddAttributeDescriptor(FacilityConstants.ServiceTypeNameKey, serviceTypeName);
        }
    }
}
