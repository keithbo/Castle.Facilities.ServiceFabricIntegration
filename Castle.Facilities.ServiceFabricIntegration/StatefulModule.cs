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

    internal class StatefulModule : IServiceFabricModule
    {
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
            Helpers.MakeWrapper(handler, typeof(StatefulWrapper<>), handler.GetProperty(FacilityConstants.ServiceTypeNameKey))
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

        public class StatefulWrapper<TService> : WrapperBase
            where TService : StatefulServiceBase
        {
            private readonly string _serviceTypeName;

            public StatefulWrapper(string serviceTypeName)
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
                        ServiceEventSource.Current.Message("Failed to resolve StatefulService type {0}.\n{1}", typeof(TService), e);
                        throw;
                    }
                });
            }
        }
    }
}
