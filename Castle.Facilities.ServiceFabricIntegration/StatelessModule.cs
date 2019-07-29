﻿namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;
    using System.Diagnostics;
    using System.Fabric;
    using System.Threading.Tasks;
    using Castle.Core;
    using Castle.Core.Internal;
    using Castle.MicroKernel;
    using Castle.MicroKernel.SubSystems.Conversion;
    using Microsoft.ServiceFabric.Services.Runtime;

    internal class StatelessModule : IServiceFabricModule
    {
        public void Init(IKernel kernel)
        {
        }

        public void Contribute(IKernel kernel, ComponentModel model)
        {
            var serviceNameFlag = HasServiceTypeNameAttributeSet(model);

            var statelessFlag = IsStatelessType(model) && HasStatelessAttributeSet(model, kernel.GetConversionManager());

            model.SetProperty(FacilityConstants.StatelessServiceKey, serviceNameFlag && statelessFlag);

            if (serviceNameFlag && statelessFlag)
            {
                model.SetProperty(FacilityConstants.ServiceTypeNameKey, model.GetAttribute(FacilityConstants.ServiceTypeNameKey));
            }
        }

        public bool CanRegister(IHandler handler)
        {
            return Helpers.IsFlag(handler, FacilityConstants.StatelessServiceKey);
        }

        public void RegisterComponent(IKernel kernel, IHandler handler)
        {
            new StatelessWrapper(
                    handler.GetProperty<string>(FacilityConstants.ServiceTypeNameKey),
                    handler.ComponentModel.Implementation)
                .RegisterAsync(kernel)
                .GetAwaiter()
                .GetResult();

            ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, handler.ComponentModel.Implementation.Name);
        }

        private static bool HasServiceTypeNameAttributeSet(ComponentModel model)
        {
            return !string.IsNullOrEmpty(model.GetAttribute(FacilityConstants.ServiceTypeNameKey));
        }

        private static bool IsStatelessType(ComponentModel model)
        {
            return model.Implementation.Is<StatelessService>();
        }

        private static bool HasStatelessAttributeSet(ComponentModel model, ITypeConverter converter)
        {
            return Helpers.IsFlag(model, converter, FacilityConstants.StatelessServiceKey);
        }

        public class StatelessWrapper : IRegistrationWrapper
        {
            private readonly string _serviceTypeName;
            private readonly Type _serviceType;

            public StatelessWrapper(string serviceTypeName, Type serviceType)
            {
                _serviceTypeName = serviceTypeName;
                _serviceType = serviceType;
            }

            public Task RegisterAsync(IKernel kernel)
            {
                return ServiceRuntime.RegisterServiceAsync(_serviceTypeName, ctx =>
                {
                    try
                    {
                        var arguments = new Arguments();
                        arguments.AddTyped<StatelessServiceContext>(ctx);

                        return (StatelessService)kernel.Resolve(_serviceType, arguments);
                    }
                    catch (Exception e)
                    {
                        ServiceEventSource.Current.Message("Failed to resolve StatelessService type {0}.\n{1}", _serviceType, e);
                        throw;
                    }
                });
            }
        }
    }
}
