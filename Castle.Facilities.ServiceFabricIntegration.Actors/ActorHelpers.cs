namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;
    using Castle.MicroKernel;
    using Microsoft.ServiceFabric.Actors.Runtime;

    internal static class ActorHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="ComponentRegistrationException"></exception>
        public static void ValidateServiceType(Type type)
        {
            if (!typeof(ActorService).IsAssignableFrom(type))
            {
                throw new ComponentRegistrationException($"Type {type} does not extend ActorService");
            }
        }
    }
}