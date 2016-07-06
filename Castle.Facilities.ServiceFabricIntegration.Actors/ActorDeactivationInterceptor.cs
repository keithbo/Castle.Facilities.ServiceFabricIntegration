namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;
    using Castle.DynamicProxy;
    using Castle.MicroKernel;
    using Microsoft.ServiceFabric.Actors.Runtime;

    /// <summary>
    /// <see cref="IInterceptor"/> implementation that wraps an Actor instance and calls <see cref="IKernel.ReleaseComponent"/> upon
    /// completion of <see cref="ActorBase.OnDeactivateAsync"/>
    /// </summary>
    public class ActorDeactivationInterceptor : IInterceptor
    {
        private readonly IKernel _kernel;

        /// <summary>
        /// Constructs a new ActorDeactivationInterceptor
        /// </summary>
        /// <param name="kernel"><see cref="IKernel"/></param>
        public ActorDeactivationInterceptor(IKernel kernel)
        {
            _kernel = kernel;
        }

        /// <summary>
        /// Intercepts calls to the proxy instance.
        /// </summary>
        /// <param name="invocation"><see cref="IInvocation"/></param>
        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();
            if (string.Equals(invocation.MethodInvocationTarget.Name, "OnDeactivateAsync", StringComparison.OrdinalIgnoreCase))
            {
                _kernel.ReleaseComponent(invocation.Proxy);
            }
        }
    }
}
