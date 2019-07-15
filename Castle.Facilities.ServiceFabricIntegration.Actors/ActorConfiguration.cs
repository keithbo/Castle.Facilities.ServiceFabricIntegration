namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;
    using Castle.MicroKernel;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class ActorConfiguration : IActorConfigurer
    {
        public Type ServiceType { get; private set; } = typeof(ActorService);

        public IActorConfigurer WithService<TService>()
        {
            var type = typeof(TService);
            ValidateServiceType(type);
            ServiceType = type;

            return this;
        }

        public IActorConfigurer WithService(Type serviceType)
        {
            ValidateServiceType(serviceType);
            ServiceType = serviceType;

            return this;
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