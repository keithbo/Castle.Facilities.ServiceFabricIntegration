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

            model.ExtendedProperties[FacilityConstants.StatefulServiceKey] = serviceNameFlag && statefulFlag;

            if (serviceNameFlag && statefulFlag)
            {
                model.ExtendedProperties[FacilityConstants.ServiceTypeNameKey] = model.Configuration.Attributes[FacilityConstants.ServiceTypeNameKey];
            }
        }

        public bool CanRegister(IHandler handler)
        {
            return Helpers.IsFlag(handler, FacilityConstants.StatefulServiceKey);
        }

        public void RegisterComponent(IKernel kernel, IHandler handler)
        {
            var stateManagerConfig = handler.ComponentModel.ExtendedProperties[typeof(ReliableStateManagerConfiguration)] as ReliableStateManagerConfiguration;
            new StatefulWrapper(
                (string)handler.GetProperty(FacilityConstants.ServiceTypeNameKey),
                handler.ComponentModel.Implementation,
                stateManagerConfig)
                .RegisterAsync(kernel)
                .GetAwaiter()
                .GetResult();

            ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, handler.ComponentModel.Implementation.Name);
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

        public class StatefulWrapper : WrapperBase
        {
            private readonly string _serviceTypeName;
            private readonly Type _serviceType;
            private readonly ReliableStateManagerConfiguration _stateManagerConfiguration;

            public StatefulWrapper(string serviceTypeName, Type serviceType, ReliableStateManagerConfiguration stateManagerConfiguration)
            {
                _serviceTypeName = serviceTypeName;
                _serviceType = serviceType;
                _stateManagerConfiguration = stateManagerConfiguration;
            }

            public override Task RegisterAsync(IKernel kernel)
            {
                return ServiceRuntime.RegisterServiceAsync(_serviceTypeName, ctx =>
                {
                    try
                    {
                        var arguments = new Arguments();
                        arguments.AddTyped<StatefulServiceContext>(ctx);
                        if (_stateManagerConfiguration != null)
                        {
                            arguments.AddTyped<IReliableStateManagerReplica>(new ReliableStateManager(ctx, _stateManagerConfiguration));
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
