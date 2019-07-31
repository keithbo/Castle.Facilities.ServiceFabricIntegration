namespace Castle.Facilities.ServiceFabricIntegration.Resolvers
{
    using System;
    using System.Fabric;
    using Castle.MicroKernel;
    using Microsoft.ServiceFabric.Services.Runtime;

    public class StatelessServiceResolver
    {
        private readonly IKernel _kernel;
        private readonly Type _serviceType;

        public StatelessServiceResolver(IKernel kernel, Type serviceType)
        {
            if (!typeof(StatelessService).IsAssignableFrom(serviceType))
            {
                throw new ArgumentException($"Type {serviceType} must extend type StatelessService");
            }

            _kernel = kernel;
            _serviceType = serviceType;
        }

        public StatelessService Resolve(StatelessServiceContext ctx)
        {
            try
            {
                var arguments = new Arguments();
                arguments.AddTyped(ctx);

                return (StatelessService)_kernel.Resolve(_serviceType, arguments);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.Message("Failed to resolve StatelessService type {0}.\n{1}", _serviceType, e);
                throw;
            }
        }
    }
}