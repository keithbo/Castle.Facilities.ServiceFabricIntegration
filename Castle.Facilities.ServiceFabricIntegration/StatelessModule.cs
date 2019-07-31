namespace Castle.Facilities.ServiceFabricIntegration
{
    using System.Diagnostics;
    using Castle.Core;
    using Castle.Core.Internal;
    using Castle.Facilities.ServiceFabricIntegration.Resolvers;
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
            var model = handler.ComponentModel;

            var serviceResolver = new StatelessServiceResolver(kernel, model.Implementation);

            ServiceRuntime.RegisterServiceAsync(
                    handler.GetProperty<string>(FacilityConstants.ServiceTypeNameKey),
                    serviceResolver.Resolve)
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
    }
}
