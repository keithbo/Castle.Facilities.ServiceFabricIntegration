namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Castle.Core;
    using Castle.Core.Internal;
    using Castle.MicroKernel;
    using Castle.MicroKernel.SubSystems.Conversion;
    using Microsoft.ServiceFabric.Services.Runtime;

    internal class StatelessModule : IServiceFabricModule
    {
        public void Contribute(IKernel kernel, ComponentModel model)
        {
            var serviceNameFlag = HasServiceTypeNameAttributeSet(model);

            var statelessFlag = IsStatelessType(model) && HasStatelessAttributeSet(model, kernel.GetConversionManager());

            model.ExtendedProperties[FacilityConstants.StatelessServiceKey] = serviceNameFlag && statelessFlag;

            if (serviceNameFlag && statelessFlag)
            {
                model.ExtendedProperties[FacilityConstants.ServiceTypeNameKey] = model.Configuration.Attributes[FacilityConstants.ServiceTypeNameKey];
            }
        }

        public bool CanRegister(IHandler handler)
        {
            return Helpers.IsFlag(handler, FacilityConstants.StatelessServiceKey);
        }

        public void RegisterComponent(IKernel kernel, IHandler handler)
        {
            Helpers.MakeWrapper(handler, typeof(StatelessWrapper<>), handler.GetProperty(FacilityConstants.ServiceTypeNameKey))
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

        public class StatelessWrapper<TService> : WrapperBase
            where TService : StatelessService
        {
            private readonly string _serviceTypeName;

            public StatelessWrapper(string serviceTypeName)
            {
                _serviceTypeName = serviceTypeName;
            }

            public override Task RegisterAsync(IKernel kernel)
            {
                return ServiceRuntime.RegisterServiceAsync(_serviceTypeName, ctx =>
                {
                    try
                    {
                        return kernel.Resolve<TService>();
                    }
                    catch (Exception e)
                    {
                        ServiceEventSource.Current.Message("Failed to resolve StatelessService type {0}.\n{1}", typeof(TService), e);
                        throw;
                    }
                });
            }
        }
    }
}
