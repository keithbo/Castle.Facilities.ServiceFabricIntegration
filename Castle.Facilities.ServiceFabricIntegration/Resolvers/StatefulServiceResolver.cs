namespace Castle.Facilities.ServiceFabricIntegration.Resolvers
{
    using System;
    using System.Fabric;
    using Castle.MicroKernel;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Services.Runtime;

    public class StatefulServiceResolver
    {
        private readonly IKernel _kernel;
        private readonly Type _serviceType;

        public ReliableStateManagerConfiguration StateManagerConfiguration { get; set; }
        public Type StateManagerDependencyType { get; set; }

        public StatefulServiceResolver(IKernel kernel, Type serviceType)
        {
            if (!typeof(StatefulServiceBase).IsAssignableFrom(serviceType))
            {
                throw new ArgumentException($"Type {serviceType} must extend type StatefulServiceBase");
            }

            _kernel = kernel;
            _serviceType = serviceType;
        }

        public StatefulServiceBase Resolve(StatefulServiceContext ctx)
        {
            try
            {
                var arguments = new Arguments();
                arguments.AddTyped(ctx);
                if (StateManagerConfiguration != null)
                {
                    arguments.AddTyped(StateManagerDependencyType, new ReliableStateManager(ctx, StateManagerConfiguration));
                }

                return (StatefulServiceBase)_kernel.Resolve(_serviceType, arguments);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.Message("Failed to resolve StatefulService type {0}.\n{1}", _serviceType, e);
                throw;
            }
        }
    }
}