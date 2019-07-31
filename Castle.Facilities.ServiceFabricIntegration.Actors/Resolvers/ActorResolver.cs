namespace Castle.Facilities.ServiceFabricIntegration.Resolvers
{
    using System;
    using Castle.MicroKernel;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class ActorResolver
    {
        private readonly IKernel _kernel;
        private readonly Type _actorType;

        public ActorResolver(IKernel kernel, Type actorType)
        {
            if (!typeof(ActorBase).IsAssignableFrom(actorType))
            {
                throw new ArgumentException($"Type {actorType} must extend type ActorBase");
            }

            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _actorType = actorType;
        }

        public ActorBase Resolve(ActorService actorService, ActorId actorId)
        {
            try
            {
                return (ActorBase)_kernel.Resolve(
                    _actorType,
                    new Arguments()
                        .AddTyped(actorService)
                        .AddTyped(actorId)
                );
            }
            catch (Exception e)
            {
                ActorEventSource.Current.Message("Failed to resolve Actor type {0}.\n{1}", _actorType, e);
                throw;
            }
        }
    }
}