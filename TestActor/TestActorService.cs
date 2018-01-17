namespace TestActor
{
    using System;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class TestActorService : ActorService
    {
        public TestActorService(
            StatefulServiceContext context,
            ActorTypeInformation actorTypeInfo,
            Func<ActorService, ActorId, ActorBase> actorFactory = null,
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null,
            IActorStateProvider stateProvider = null,
            ActorServiceSettings settings = null)
            : base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
        {
        }

        protected override Task RunAsync(CancellationToken cancellationToken)
        {
            ActorEventSource.Current.Message("TestActorService.RunAsync()");
            return base.RunAsync(cancellationToken);
        }
    }
}