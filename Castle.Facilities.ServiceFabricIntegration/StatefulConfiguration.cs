namespace Castle.Facilities.ServiceFabricIntegration
{
    using Castle.MicroKernel;
    using Castle.MicroKernel.Registration;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Services.Runtime;

    public class StatefulConfiguration<TService> : IStatefulConfigurer
        where TService : StatefulServiceBase
    {
        private readonly ComponentRegistration<TService> _registration;

        public StatefulConfiguration(ComponentRegistration<TService> registration)
        {
            _registration = registration;
        }

        public string ServiceTypeName { get; set; }

        public ReliableStateManagerConfiguration StateManagerConfiguration { get; set; }

        public ComponentRegistration<TService> Build()
        {
            if (string.IsNullOrEmpty(ServiceTypeName))
            {
                throw new ComponentRegistrationException("Stateful ServiceTypeName cannot be null or empty");
            }

            if (StateManagerConfiguration != null)
            {
                _registration.ExtendedProperties(
                    Property
                        .ForKey<ReliableStateManagerConfiguration>()
                        .Eq(StateManagerConfiguration));
            }

            return _registration
                .AddAttributeDescriptor(FacilityConstants.StatefulServiceKey, bool.TrueString)
                .AddAttributeDescriptor(FacilityConstants.ServiceTypeNameKey, ServiceTypeName);
        }
    }
}