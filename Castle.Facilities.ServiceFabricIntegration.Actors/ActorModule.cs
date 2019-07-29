namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;
    using System.Collections.Concurrent;
    using System.Fabric;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Castle.Core;
    using Castle.Core.Internal;
    using Castle.MicroKernel;
    using Castle.MicroKernel.SubSystems.Conversion;
    using MicroKernel.Registration;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Expression=System.Linq.Expressions.Expression;

    internal class ActorModule : IServiceFabricModule
    {
        /// <summary>
        /// Generic capture of <see cref="ActorRuntime.RegisterActorAsync{TActor}(Func{StatefulServiceContext, ActorTypeInformation, ActorService}, TimeSpan, CancellationToken)"/>
        /// to be realized upon registration of an actor type
        /// </summary>
        private static readonly MethodInfo RuntimeRegistrationMethod;

        /// <summary>
        /// cache compiled types to avoid re-compile in case of duplicate type
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Func<Func<StatefulServiceContext, ActorTypeInformation, ActorService>, Task>> RegistrationCache;

        static ActorModule()
        {
            RuntimeRegistrationMethod = typeof(ActorRuntime).GetMethod(
                "RegisterActorAsync",
                BindingFlags.Static | BindingFlags.Public,
                null,
                CallingConventions.Any,
                new Type[]
                {
                    typeof(Func<StatefulServiceContext,ActorTypeInformation,ActorService>),
                    typeof(TimeSpan),
                    typeof(CancellationToken)
                },
                new ParameterModifier[0]);

            RegistrationCache = new ConcurrentDictionary<Type, Func<Func<StatefulServiceContext, ActorTypeInformation, ActorService>, Task>>();
        }

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

            var serviceType = handler.GetProperty<Type>(FacilityConstants.ActorServiceTypeKey);
            var serviceHandler = kernel.GetHandler(serviceType);
            if (serviceHandler == null)
            {
                throw new ComponentRegistrationException($"Component for ActorService {serviceType} must be registered before Actor {actorModel.Implementation}");
            }

            var serviceModel = serviceHandler.ComponentModel;

            var stateManagerFactory = actorModel.GetProperty<Func<ActorBase, IActorStateProvider, IActorStateManager>>(typeof(Func<ActorBase, IActorStateProvider, IActorStateManager>));
            if (stateManagerFactory != null && serviceModel.GetDependencyFor(typeof(Func<ActorBase, IActorStateProvider, IActorStateManager>)) == null)
            {
                throw new ComponentRegistrationException($"Failed to register Actor {actorModel.Implementation}. Could not locate a valid dependency on {serviceType} that accepts {typeof(Func<ActorBase, IActorStateProvider, IActorStateManager>)} when StateManagerFactory delegate is set.");
            }

            var actorServiceSettings = actorModel.GetProperty<ActorServiceSettings>(typeof(ActorServiceSettings));
            if (actorServiceSettings != null && serviceModel.GetDependencyFor(typeof(ActorServiceSettings)) == null)
            {
                throw new ComponentRegistrationException($"Failed to register Actor {actorModel.Implementation}. Could not locate a valid dependency on {serviceType} that accepts {typeof(ActorServiceSettings)} when ActorServiceSettings is set.");
            }

            actorModel.Interceptors.Add(new InterceptorReference(typeof(ActorDeactivationInterceptor)));
            new ActorWrapper(serviceType, actorModel.Implementation, stateManagerFactory, actorServiceSettings)
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

        private static Type GetActorServiceTypeAttribute(ComponentModel model, ITypeConverter converter)
        {
            return Helpers.GetType(model, converter, FacilityConstants.ActorServiceTypeKey);
        }

        public class ActorWrapper : IRegistrationWrapper
        {
            private readonly Func<Func<StatefulServiceContext, ActorTypeInformation, ActorService>, Task> _registerActorAsync;
            private readonly Type _serviceType;
            private readonly Type _actorType;
            private readonly Func<ActorBase, IActorStateProvider, IActorStateManager> _stateManagerFactory;
            private readonly ActorServiceSettings _actorServiceSettings;

            public ActorWrapper(Type serviceType, Type actorType, Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory, ActorServiceSettings actorServiceSettings)
            {
                _serviceType = serviceType;
                _actorType = actorType;
                _registerActorAsync = RegistrationCache.GetOrAdd(_actorType, CreateRegistrationFunc);
                _stateManagerFactory = stateManagerFactory;
                _actorServiceSettings = actorServiceSettings;
            }

            public Task RegisterAsync(IKernel kernel)
            {
                return _registerActorAsync(CreateFactoryFunc(kernel));
            }

            private Func<StatefulServiceContext, ActorTypeInformation, ActorService> CreateFactoryFunc(IKernel kernel)
            {
                ActorBase ActorResolveFunc(ActorService actorService, ActorId actorId)
                {
                    try
                    {
                        return (ActorBase) kernel.Resolve(
                            _actorType,
                            new Arguments()
                                .AddTyped(actorService)
                                .AddTyped(actorId)
                        );
                    }
                    catch (Exception e)
                    {
                        ActorEventSource.Current.Message("Failed to resolve Actor type {0}.\n{1}", _actorType, e);
                        throw;
                    }
                }

                return (ctx, info) =>
                {
                    try
                    {
                        return (ActorService) kernel.Resolve(
                            _serviceType,
                            new Arguments()
                                .AddTyped(ctx)
                                .AddTyped(info)
                                .AddTyped<Func<ActorService, ActorId, ActorBase>>(ActorResolveFunc)
                                .AddTyped(_stateManagerFactory)
                                .AddTyped<IActorStateProvider>(null)
                                .AddTyped(_actorServiceSettings)
                        );
                    }
                    catch (Exception e)
                    {
                        ActorEventSource.Current.Message("Failed to resolve ActorService type {0}.\n{1}", _serviceType, e);
                        throw;
                    }
                };
            }

            private static Func<Func<StatefulServiceContext, ActorTypeInformation, ActorService>, Task> CreateRegistrationFunc(Type actorType)
            {
                var realizedMethodInfo = RuntimeRegistrationMethod.MakeGenericMethod(actorType);
                var factoryParam = Expression.Parameter(typeof(Func<StatefulServiceContext, ActorTypeInformation, ActorService>));
                var call = Expression.Call(null, realizedMethodInfo,
                    factoryParam,
                    Expression.Constant(default(TimeSpan)),
                    Expression.Constant(default(CancellationToken)));

                return Expression
                    .Lambda<Func<Func<StatefulServiceContext, ActorTypeInformation, ActorService>, Task>>(call, factoryParam)
                    .Compile();
            }
        }
    }
}
