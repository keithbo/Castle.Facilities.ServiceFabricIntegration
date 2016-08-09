namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;
    using System.Linq;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Facilities;

    /// <summary>
    /// Implementation of <see cref="IFacility"/> that provides registration support and integration of Castle Windsor
    /// with the ServiceFabric runtime.
    /// Default support includes Stateful and Statless service injection.
    /// Additional support modules can be added using the <see cref="IServiceFabricModule"/> interface and <see cref="Configure"/>
    /// during facility initialization.
    /// </summary>
    public class ServiceFabricFacility : AbstractFacility
    {
        private readonly ServiceFabricFacilityConfiguration _config;

        public ServiceFabricFacility()
        {
            _config = new ServiceFabricFacilityConfiguration();
            _config.Using(new StatefulModule(), new StatelessModule());
        }

        /// <summary>
        /// Configure the facility to provide additional behavior.
        /// </summary>
        /// <param name="configure">Configuration delegate</param>
        public void Configure(Action<IServiceFabricFacilityConfigurer> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            configure(_config);
        }

        protected override void Init()
        {
            _config.Modules.ForEach(m => m.Init(Kernel));

            Kernel.ComponentModelBuilder.AddContributor(new ServiceFabricContributor(_config));

            Kernel.ComponentRegistered += OnComponentRegistered;
        }

        private void OnComponentRegistered(string key, IHandler handler)
        {
            foreach (var m in _config.Modules.Where(m => m.CanRegister(handler)))
            {
                m.RegisterComponent(Kernel, handler);
            }
        }
    }
}
