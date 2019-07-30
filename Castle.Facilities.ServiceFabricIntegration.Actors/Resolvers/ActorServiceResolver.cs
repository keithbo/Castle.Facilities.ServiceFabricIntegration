namespace Castle.Facilities.ServiceFabricIntegration.Resolvers
{
    using System;
    using System.Fabric;
    using Castle.MicroKernel;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class ActorServiceResolver
    {
        private readonly IKernel _kernel;
        private readonly Type _serviceType;

        public Func<ActorService, ActorId, ActorBase> ActorFactory { get; set; }

        public Func<ActorBase, IActorStateProvider, IActorStateManager> StateManagerFactory { get; set; }

        public IActorStateProvider StateProvider { get; set; }

        public ActorServiceSettings Settings { get; set; }

        public ActorServiceResolver(IKernel kernel, Type serviceType)
        {
            if (!typeof(ActorService).IsAssignableFrom(serviceType))
            {
                throw new ArgumentException($"Type {serviceType} must extend type ActorService");
            }

            _kernel = kernel;
            _serviceType = serviceType;
        }

        public ActorService Resolve(StatefulServiceContext ctx, ActorTypeInformation info)
        {
            try
            {
                return (ActorService)_kernel.Resolve(
                    _serviceType,
                    new Arguments()
                        .AddTyped(ctx)
                        .AddTyped(info)
                        .AddTyped(ActorFactory)
                        .AddTyped(StateManagerFactory)
                        .AddTyped(StateProvider)
                        .AddTyped(Settings)
                );
            }
            catch (Exception e)
            {
                ActorEventSource.Current.Message("Failed to resolve ActorService type {0}.\n{1}", _serviceType, e);
                throw;
            }
        }
    }
}