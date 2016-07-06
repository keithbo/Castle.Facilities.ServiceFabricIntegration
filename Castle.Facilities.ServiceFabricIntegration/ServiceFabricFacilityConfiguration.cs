namespace Castle.Facilities.ServiceFabricIntegration
{
    using System.Collections.Generic;

    internal class ServiceFabricFacilityConfiguration : IServiceFabricFacilityConfigurer
    {
        public List<IServiceFabricModule> Modules { get; }

        public ServiceFabricFacilityConfiguration()
        {
            Modules = new List<IServiceFabricModule>();
        }

        public IServiceFabricFacilityConfigurer Using(params IServiceFabricModule[] modules)
        {
            Modules.AddRange(modules);
            return this;
        }
    }
}
