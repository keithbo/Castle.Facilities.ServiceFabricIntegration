namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;

    public interface IActorConfigurer
    {
        IActorConfigurer WithService<TService>();

        IActorConfigurer WithService(Type serviceType);
    }
}