namespace Castle.Facilities.ServiceFabricIntegration.Resolvers
{
    using System;
    using System.Collections.Concurrent;
    using System.Fabric;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class RuntimeRegistration
    {
        /// <summary>
        /// Generic capture of <see>
        ///     <cref>ActorRuntime.RegisterActorAsync{TActor}(Func{T1,T2,TResult}, TimeSpan, CancellationToken)</cref>
        /// </see>
        /// to be realized upon registration of an actor type
        /// </summary>
        internal static readonly MethodInfo RuntimeRegistrationMethod;

        /// <summary>
        /// cache compiled types to avoid re-compile in case of duplicate type
        /// </summary>
        internal static readonly ConcurrentDictionary<Type, Func<Func<StatefulServiceContext, ActorTypeInformation, ActorService>, Task>> RegistrationCache;

        static RuntimeRegistration()
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

        private readonly Func<Func<StatefulServiceContext, ActorTypeInformation, ActorService>, Task> _registerAsync;

        public RuntimeRegistration(Type actorType)
        {
            if (!typeof(ActorBase).IsAssignableFrom(actorType))
            {
                throw new ArgumentException($"Type {actorType} must extend type ActorBase");
            }

            if (actorType.IsAbstract)
            {
                throw new ArgumentException($"Type {actorType} cannot be abstract");
            }

            _registerAsync = RegistrationCache.GetOrAdd(actorType, CreateRegistrationFunc);
        }

        public async Task RegisterAsync(Func<StatefulServiceContext, ActorTypeInformation, ActorService> serviceFactory)
        {
            await _registerAsync(serviceFactory);
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