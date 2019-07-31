namespace Tests.Resolvers
{
    using System.Fabric;
    using Microsoft.ServiceFabric.Services.Runtime;

    public class TestStatelessService : StatelessService
    {
        /// <inheritdoc />
        public TestStatelessService(StatelessServiceContext serviceContext) : base(serviceContext)
        {
        }
    }
}