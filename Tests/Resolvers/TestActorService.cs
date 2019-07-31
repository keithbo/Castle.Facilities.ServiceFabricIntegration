namespace Tests.Resolvers
{
    using System;
    using System.Fabric;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class TestActorService : ActorService
    {
        public ActorTypeInformation ActorTypeInfo { get; }
        public Func<ActorService, ActorId, ActorBase> ActorFactory { get; }
        public Func<ActorBase, IActorStateProvider, IActorStateManager> StateManagerFactory { get; }
        public new IActorStateProvider StateProvider { get; }
        public new ActorServiceSettings Settings { get; }

        /// <inheritdoc />
        public TestActorService(
            StatefulServiceContext context,
            ActorTypeInformation actorTypeInfo,
            Func<ActorService, ActorId, ActorBase> actorFactory = null,
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null,
            IActorStateProvider stateProvider = null,
            ActorServiceSettings settings = null)
            : base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
        {
            ActorTypeInfo = actorTypeInfo;
            ActorFactory = actorFactory;
            StateManagerFactory = stateManagerFactory;
            StateProvider = stateProvider;
            Settings = settings;
        }
    }
}