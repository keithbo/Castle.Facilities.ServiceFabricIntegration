namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;
    using System.Diagnostics;
    using System.Fabric;
    using System.Threading.Tasks;
    using Castle.Core;
    using Castle.Core.Internal;
    using Castle.MicroKernel;
    using Castle.MicroKernel.SubSystems.Conversion;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Services.Runtime;

    internal class StatefulModule : IServiceFabricModule
    {
        public void Init(IKernel kernel)
        {
        }

        public void Contribute(IKernel kernel, ComponentModel model)
        {
            var serviceNameFlag = HasServiceTypeNameAttributeSet(model);

            var statefulFlag = IsStatefulType(model) && HasStatefulAttributeSet(model, kernel.GetConversionManager());

            model.SetProperty(FacilityConstants.StatefulServiceKey, serviceNameFlag && statefulFlag);

            if (serviceNameFlag && statefulFlag)
            {
                model.SetProperty(FacilityConstants.ServiceTypeNameKey, model.GetAttribute(FacilityConstants.ServiceTypeNameKey));
            }
        }

        public bool CanRegister(IHandler handler)
        {
            return Helpers.IsFlag(handler, FacilityConstants.StatefulServiceKey);
        }

        public void RegisterComponent(IKernel kernel, IHandler handler)
        {
            var model = handler.ComponentModel;

            Type stateManagerDependencyType = null;
            var stateManagerConfig = model.GetProperty<ReliableStateManagerConfiguration>(typeof(ReliableStateManagerConfiguration));
            if (!(stateManagerConfig == null || model.TryGetDependencyType<ReliableStateManager>(out stateManagerDependencyType)))
            {
                throw new ComponentRegistrationException($"Failed to register StatefulService {model.Implementation}. Could not locate a valid dependency input that accepts {typeof(ReliableStateManager)} when an explicit state manager configuration is specified.");
            }

            new StatefulWrapper(
                    handler.GetProperty<string>(FacilityConstants.ServiceTypeNameKey),
                    model.Implementation,
                    stateManagerConfig,
                    stateManagerDependencyType)
                .RegisterAsync(kernel)
                .GetAwaiter()
                .GetResult();

            ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, model.Implementation.Name);
        }

        private static bool IsStatefulType(ComponentModel model)
        {
            return model.Implementation.Is<StatefulServiceBase>();
        }

        private static bool HasServiceTypeNameAttributeSet(ComponentModel model)
        {
            return !string.IsNullOrEmpty(model.GetAttribute(FacilityConstants.ServiceTypeNameKey));
        }

        private static bool HasStatefulAttributeSet(ComponentModel model, ITypeConverter converter)
        {
            return Helpers.IsFlag(model, converter, FacilityConstants.StatefulServiceKey);
        }

        public class StatefulWrapper : IRegistrationWrapper
        {
            private readonly string _serviceTypeName;
            private readonly Type _serviceType;
            private readonly ReliableStateManagerConfiguration _stateManagerConfiguration;
            private readonly Type _stateManagerDependencyType;

            public StatefulWrapper(string serviceTypeName, Type serviceType, ReliableStateManagerConfiguration stateManagerConfiguration, Type stateManagerDependencyType)
            {
                _serviceTypeName = serviceTypeName;
                _serviceType = serviceType;
                _stateManagerConfiguration = stateManagerConfiguration;
                _stateManagerDependencyType = stateManagerDependencyType;
            }

            public Task RegisterAsync(IKernel kernel)
            {
                return ServiceRuntime.RegisterServiceAsync(_serviceTypeName, ctx =>
                {
                    try
                    {
                        var arguments = new Arguments();
                        arguments.AddTyped<StatefulServiceContext>(ctx);
                        if (_stateManagerConfiguration != null)
                        {
                            arguments.AddTyped(_stateManagerDependencyType, new ReliableStateManager(ctx, _stateManagerConfiguration));
                        }

                        return (StatefulServiceBase)kernel.Resolve(_serviceType, arguments);
                    }
                    catch (Exception e)
                    {
                        ServiceEventSource.Current.Message("Failed to resolve StatefulService type {0}.\n{1}", _serviceType, e);
                        throw;
                    }
                });
            }
        }
    }
}
