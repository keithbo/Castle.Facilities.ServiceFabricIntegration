namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;
    using Castle.Core;
    using Castle.Core.Internal;
    using Castle.Facilities.ServiceFabricIntegration.Resolvers;
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
                    .LifestyleTransient(),
                Component.For<ActorService>()
                    .LifestyleTransient()
                    .DependsOn(Dependency.OnValue("stateManagerFactory", null))
                    .DependsOn(Dependency.OnValue("stateProvider", null))
                    .DependsOn(Dependency.OnValue("settings", null))
            );
        }

        public void Contribute(IKernel kernel, ComponentModel model)
        {
            var actorFlag = IsActorType(model) &&
                            HasActorAttributeSet(model, kernel.GetConversionManager());

            model.SetProperty(FacilityConstants.ActorKey, actorFlag);
            if (actorFlag)
            {
                var actorServiceType = GetActorServiceTypeAttribute(model, kernel.GetConversionManager()) ?? typeof(ActorService);
                model.SetProperty(FacilityConstants.ActorServiceTypeKey, actorServiceType);
            }
        }

        public bool CanRegister(IHandler handler)
        {
            return Helpers.IsFlag(handler, FacilityConstants.ActorKey);
        }

        public void RegisterComponent(IKernel kernel, IHandler handler)
        {
            var actorModel = handler.ComponentModel;
            var actorType = actorModel.Implementation;

            var serviceType = handler.GetProperty<Type>(FacilityConstants.ActorServiceTypeKey);
            var serviceHandler = kernel.GetHandler(serviceType);
            if (serviceHandler == null)
            {
                throw new ComponentRegistrationException($"Component for ActorService {serviceType} must be registered before Actor {actorType}");
            }

            var serviceModel = serviceHandler.ComponentModel;

            var stateManagerFactory = actorModel.GetProperty<Func<ActorBase, IActorStateProvider, IActorStateManager>>(typeof(Func<ActorBase, IActorStateProvider, IActorStateManager>));
            if (stateManagerFactory != null && serviceModel.GetDependencyFor(typeof(Func<ActorBase, IActorStateProvider, IActorStateManager>)) == null)
            {
                throw new ComponentRegistrationException($"Failed to register Actor {actorType}. Could not locate a valid dependency on {serviceType} that accepts {typeof(Func<ActorBase, IActorStateProvider, IActorStateManager>)} when StateManagerFactory delegate is set.");
            }

            var actorServiceSettings = actorModel.GetProperty<ActorServiceSettings>(typeof(ActorServiceSettings));
            if (actorServiceSettings != null && serviceModel.GetDependencyFor(typeof(ActorServiceSettings)) == null)
            {
                throw new ComponentRegistrationException($"Failed to register Actor {actorType}. Could not locate a valid dependency on {serviceType} that accepts {typeof(ActorServiceSettings)} when ActorServiceSettings is set.");
            }

            actorModel.Interceptors.Add(new InterceptorReference(typeof(ActorDeactivationInterceptor)));
            var serviceResolver = new ActorServiceResolver(kernel, serviceType)
            {
                ActorFactory = new ActorResolver(kernel, actorType).Resolve,
                StateManagerFactory = stateManagerFactory,
                StateProvider = null,
                Settings = actorServiceSettings
            };

            new RuntimeRegistration(actorType)
                .RegisterAsync(serviceResolver.Resolve)
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

        private static Type GetActorServiceTypeAttribute(ComponentModel model, ITypeConverter converter)
        {
            return Helpers.GetType(model, converter, FacilityConstants.ActorServiceTypeKey);
        }
    }
}
