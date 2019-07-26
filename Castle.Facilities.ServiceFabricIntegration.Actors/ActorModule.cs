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
                    .DependsOn(Dependency.OnValue("settings", null)));
        }

        public void Contribute(IKernel kernel, ComponentModel model)
        {
            var actorFlag = IsActorType(model) &&
                            HasActorAttributeSet(model, kernel.GetConversionManager());

            model.ExtendedProperties[FacilityConstants.ActorKey] = actorFlag;
            if (actorFlag)
            {
                var actorServiceType = GetActorServiceTypeAttribute(model, kernel.GetConversionManager()) ?? typeof(ActorService);
                model.ExtendedProperties[FacilityConstants.ActorServiceTypeKey] = actorServiceType;
            }
        }

        public bool CanRegister(IHandler handler)
        {
            return Helpers.IsFlag(handler, FacilityConstants.ActorKey);
        }

        public void RegisterComponent(IKernel kernel, IHandler handler)
        {
            handler.ComponentModel.Interceptors.Add(new InterceptorReference(typeof(ActorDeactivationInterceptor)));
            new ActorWrapper(handler.ComponentModel.Implementation)
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
            private readonly Type _actorType;

            public ActorWrapper(Type actorType)
            {
                _actorType = actorType;
                _registerActorAsync = RegistrationCache.GetOrAdd(_actorType, CreateRegistrationFunc);
            }

            public Task RegisterAsync(IKernel kernel)
            {
                var serviceType = kernel.GetHandler(_actorType).GetProperty<Type>(FacilityConstants.ActorServiceTypeKey);

                return _registerActorAsync(CreateFactoryFunc(kernel, serviceType, _actorType));
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

            private static Func<StatefulServiceContext, ActorTypeInformation, ActorService> CreateFactoryFunc(IKernel kernel, Type serviceType, Type actorType)
            {
                ActorBase ActorResolveFunc(ActorService actorService, ActorId actorId)
                {
                    try
                    {
                        return (ActorBase) kernel.Resolve(
                            actorType,
                            new Arguments()
                                .AddTyped<ActorService>(actorService)
                                .AddTyped<ActorId>(actorId)
                        );
                    }
                    catch (Exception e)
                    {
                        ActorEventSource.Current.Message("Failed to resolve Actor type {0}.\n{1}", actorType, e);
                        throw;
                    }
                }

                return (ctx, info) =>
                {
                    try
                    {
                        return (ActorService) kernel.Resolve(
                            serviceType,
                            new Arguments()
                                .AddTyped<StatefulServiceContext>(ctx)
                                .AddTyped<ActorTypeInformation>(info)
                                .AddTyped<Func<ActorService, ActorId, ActorBase>>(ActorResolveFunc)
                        );
                    }
                    catch (Exception e)
                    {
                        ActorEventSource.Current.Message("Failed to resolve ActorService type {0}.\n{1}", serviceType, e);
                        throw;
                    }
                };
            }
        }
    }
}
