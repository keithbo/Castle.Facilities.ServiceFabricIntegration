namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public interface IActorConfigurer
    {
        IActorConfigurer WithService<TService>();

        IActorConfigurer WithService(Type serviceType);

        Func<ActorBase, IActorStateProvider, IActorStateManager> StateManagerFactory { get; set; }

        ActorServiceSettings ServiceSettings { get; set; }
    }
}