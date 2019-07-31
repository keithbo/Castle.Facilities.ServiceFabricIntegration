namespace Tests.Resolvers
{
    using System.Fabric;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Services.Runtime;

    public class TestStatefulService : StatefulService
    {
        public IReliableStateManagerReplica ReliableStateManagerReplica { get; }

        /// <inheritdoc />
        public TestStatefulService(StatefulServiceContext serviceContext) : base(serviceContext)
        {
        }

        /// <inheritdoc />
        public TestStatefulService(StatefulServiceContext serviceContext, IReliableStateManagerReplica reliableStateManagerReplica) : base(serviceContext, reliableStateManagerReplica)
        {
            ReliableStateManagerReplica = reliableStateManagerReplica;
        }
    }
}