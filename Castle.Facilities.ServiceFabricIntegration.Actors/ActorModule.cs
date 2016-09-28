namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;
    using System.Threading.Tasks;
    using Castle.Core;
    using Castle.Core.Internal;
    using Castle.MicroKernel;
    using Castle.MicroKernel.SubSystems.Conversion;
    using MicroKernel.Registration;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;

    internal class ActorModule : IServiceFabricModule
    {
        public void Init(IKernel kernel)
        {
            kernel.Register(
                Component.For<ActorDeactivationInterceptor>()
                    .LifestyleTransient());
        }

        public void Contribute(IKernel kernel, ComponentModel model)
        {
            var actorFlag = IsActorType(model) && HasActorAttributeSet(model, kernel.GetConversionManager());

            model.ExtendedProperties[FacilityConstants.ActorKey] = actorFlag;
        }

        public bool CanRegister(IHandler handler)
        {
            return Helpers.IsFlag(handler, FacilityConstants.ActorKey);
        }

        public void RegisterComponent(IKernel kernel, IHandler handler)
        {
            handler.ComponentModel.Interceptors.Add(new InterceptorReference(typeof(ActorDeactivationInterceptor)));
            Helpers.MakeWrapper(handler, typeof(ActorWrapper<>))
                .RegisterAsync(kernel)
                .GetAwaiter()
                .GetResult();
        }

        private static bool IsActorType(ComponentModel model)
        {
            return model.Implementation.Is<IActor>();
        }

        private static bool HasActorAttributeSet(ComponentModel model, ITypeConverter converter)
        {
            return Helpers.IsFlag(model, converter, FacilityConstants.ActorKey);
        }

        public class ActorWrapper<TActor> : WrapperBase
            where TActor : ActorBase
        {
            public override Task RegisterAsync(IKernel kernel)
            {
                return ActorRuntime.RegisterActorAsync<TActor>((ctx, info) =>
                    new ActorService(ctx, info, (actorService, actorId) =>
                    {
                        try
                        {
                            return kernel.Resolve<TActor>(new
                            {
                                actorService,
                                actorId
                            });
                        }
                        catch (Exception e)
                        {
                            ActorEventSource.Current.Message("Failed to resolve Actor type {0}.\n{1}", typeof(TActor), e);
                            throw;
                        }
                    }));
            }
        }
    }
}
