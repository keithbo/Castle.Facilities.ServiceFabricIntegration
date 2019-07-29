namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Registration;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class ActorConfiguration<TActor> : IActorConfigurer
        where TActor : ActorBase
    {
        private readonly ComponentRegistration<TActor> _registration;

        public Type ServiceType { get; private set; } = typeof(ActorService);

        public Func<ActorBase, IActorStateProvider, IActorStateManager> StateManagerFactory { get; set; }

        public ActorServiceSettings ServiceSettings { get; set; }

        public ActorConfiguration(ComponentRegistration<TActor> registration)
        {
            _registration = registration;
        }

        public IActorConfigurer WithService<TService>()
        {
            var type = typeof(TService);
            ServiceType = type;

            return this;
        }

        public IActorConfigurer WithService(Type serviceType)
        {
            ServiceType = serviceType;

            return this;
        }

        public ComponentRegistration<TActor> Build()
        {
            ValidateServiceType(ServiceType);

            return _registration
                .AddAttributeDescriptor(FacilityConstants.ActorKey, bool.TrueString)
                .AddAttributeDescriptor(FacilityConstants.ActorServiceTypeKey, ServiceType.AssemblyQualifiedName)
                .ExtendedProperties(
                    Property.ForKey<Func<ActorBase, IActorStateProvider, IActorStateManager>>().Eq(StateManagerFactory),
                    Property.ForKey<ActorServiceSettings>().Eq(ServiceSettings));
        }

        internal static void ValidateServiceType(Type type)
        {
            if (!typeof(ActorService).IsAssignableFrom(type))
            {
                throw new ComponentRegistrationException($"Type {type} does not extend ActorService");
            }
        }
    }
}