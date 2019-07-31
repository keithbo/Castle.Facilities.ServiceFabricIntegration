namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;
    using System.Diagnostics;
    using Castle.Core;
    using Castle.Core.Internal;
    using Castle.Facilities.ServiceFabricIntegration.Resolvers;
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

            var serviceResolver = new StatefulServiceResolver(kernel, model.Implementation)
            {
                StateManagerConfiguration = stateManagerConfig,
                StateManagerDependencyType = stateManagerDependencyType
            };

            ServiceRuntime.RegisterServiceAsync(
                    handler.GetProperty<string>(FacilityConstants.ServiceTypeNameKey),
                    serviceResolver.Resolve)
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
    }
}
