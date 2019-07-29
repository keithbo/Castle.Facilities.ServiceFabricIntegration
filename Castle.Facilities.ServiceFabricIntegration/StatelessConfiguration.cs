namespace Castle.Facilities.ServiceFabricIntegration
{
    using Castle.MicroKernel;
    using Castle.MicroKernel.Registration;
    using Microsoft.ServiceFabric.Services.Runtime;

    public class StatelessConfiguration<TService>
        where TService : StatelessService
    {
        private readonly ComponentRegistration<TService> _registration;

        public string ServiceTypeName { get; set; }

        public StatelessConfiguration(ComponentRegistration<TService> registration)
        {
            _registration = registration;
        }

        public ComponentRegistration<TService> Build()
        {
            if (string.IsNullOrEmpty(ServiceTypeName))
            {
                throw new ComponentRegistrationException("Stateless ServiceTypeName cannot be null or empty");
            }

            return _registration
                .AddAttributeDescriptor(FacilityConstants.StatelessServiceKey, bool.TrueString)
                .AddAttributeDescriptor(FacilityConstants.ServiceTypeNameKey, ServiceTypeName);
        }
    }
}