namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public interface IActorConfigurer
    {
        Type ServiceType { get; set; }

        Func<ActorBase, IActorStateProvider, IActorStateManager> StateManagerFactory { get; set; }

        ActorServiceSettings ServiceSettings { get; set; }
    }
}