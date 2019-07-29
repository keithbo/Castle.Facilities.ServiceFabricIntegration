namespace Castle.Facilities.ServiceFabricIntegration
{
    using Microsoft.ServiceFabric.Data;

    public interface IStatefulConfigurer
    {
        ReliableStateManagerConfiguration StateManagerConfiguration { get; set; }
    }
}