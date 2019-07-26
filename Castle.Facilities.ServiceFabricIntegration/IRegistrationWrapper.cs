namespace Castle.Facilities.ServiceFabricIntegration
{
    using System.Threading.Tasks;
    using Castle.MicroKernel;

    /// <summary>
    /// WrapperBase provides a simple extensible mechanism for boxing service fabric registration
    /// tasks without requiring reflection of generic types.
    /// Derivative types are free to be generic while the base <see cref="RegisterAsync"/> abstract does not
    /// require access to the generic types to be called.
    /// </summary>
    /// <remarks>For example, this is used internally to allow generic registration of Stateless and Stateful service types without losing the generic type arguments</remarks>
    public interface IRegistrationWrapper
    {
        /// <summary>
        /// Register using the provided IKernel.
        /// </summary>
        /// <param name="kernel"><see cref="IKernel"/></param>
        /// <returns><see cref="Task"/></returns>
        Task RegisterAsync(IKernel kernel);
    }
}
