namespace Castle.Facilities.ServiceFabricIntegration
{
    using Castle.Core;
    using Castle.MicroKernel;
    using Castle.MicroKernel.ModelBuilder;

    internal class ServiceFabricContributor : IContributeComponentModelConstruction
    {
        private readonly ServiceFabricFacilityConfiguration _config;

        public ServiceFabricContributor(ServiceFabricFacilityConfiguration config)
        {
            _config = config;
        }

        public void ProcessModel(IKernel kernel, ComponentModel model)
        {
            foreach (var m in _config.Modules)
            {
                m.Contribute(kernel, model);
            }
        }
    }
}
