namespace Castle.Facilities.ServiceFabricIntegration
{
    /// <summary>
    /// Configuration interface for <see cref="ServiceFabricFacility"/>
    /// </summary>
    public interface IServiceFabricFacilityConfigurer
    {
        /// <summary>
        /// Provide additional modules for <see cref="ServiceFabricFacility"/> to include in the configuration process.
        /// </summary>
        /// <param name="modules">Array of <see cref="IServiceFabricModule"/> instances</param>
        /// <returns><see cref="IServiceFabricFacilityConfigurer"/> for continued configuration</returns>
        IServiceFabricFacilityConfigurer Using(params IServiceFabricModule[] modules);
    }
}
