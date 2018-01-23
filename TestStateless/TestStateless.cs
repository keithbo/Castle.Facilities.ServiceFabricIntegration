using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace TestStateless
{
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Client;
    using TestActor.Interfaces;

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class TestStateless : StatelessService
    {
        public TestStateless(StatelessServiceContext serviceContext)
            : base(serviceContext)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                // TODO: Replace the following sample code with your own logic 
                //       or remove this RunAsync override if it's not needed in your service.

                long iterations = 0;
                ITestActor actor = ActorProxy.Create<ITestActor>(ActorId.CreateRandom());
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    ServiceEventSource.Current.ServiceMessage(this.Context, $"{nameof(TestStateless)} Working-{++iterations}");
                    await actor.SetCountAsync((int)iterations, cancellationToken);

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"Exception: {ex}");
                throw;
            }
        }
    }
}
