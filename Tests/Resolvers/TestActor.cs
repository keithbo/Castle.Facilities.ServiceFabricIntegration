namespace Tests.Resolvers
{
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public interface ITestActor : IActor
    {

    }

    public class TestActor : Actor, ITestActor
    {
        public ActorId ActorId { get; }

        /// <inheritdoc />
        public TestActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
        {
            ActorId = actorId;
        }
    }
}