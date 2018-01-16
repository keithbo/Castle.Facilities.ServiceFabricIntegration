namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;
    using Castle.Core;
    using Castle.MicroKernel;
    using Castle.MicroKernel.SubSystems.Conversion;

    /// <summary>
    /// Helpers for managing facility integration
    /// </summary>
    public static class Helpers
    {
        public static bool IsFlag(IHandler handler, string flagName)
        {
            var obj = handler.ComponentModel.ExtendedProperties[flagName];
            return ((bool?)obj).GetValueOrDefault();
        }

        public static bool IsFlag(ComponentModel model, ITypeConverter converter, string attributeName)
        {
            if (model.Configuration == null)
            {
                return false;
            }

            var text = model.Configuration.Attributes[attributeName];
            return text != null && converter.PerformConversion<bool>(text);
        }

        public static Type GetType(ComponentModel model, ITypeConverter converter, string attributeName)
        {
            var text = model.Configuration?.Attributes[attributeName];
            return text == null ? null : converter.PerformConversion<Type>(text);
        }

        public static WrapperBase MakeWrapper(IHandler handler, Type wrapperType, params object[] args)
        {
            return (WrapperBase)Activator.CreateInstance(wrapperType.MakeGenericType(handler.ComponentModel.Implementation), args);
        }

        public static object GetProperty(this IHandler handler, string name)
        {
            return handler.ComponentModel.ExtendedProperties[name];
        }

        public static T GetProperty<T>(this IHandler handler, string name)
        {
            return (T)handler.ComponentModel.ExtendedProperties[name];
        }

        public static string GetAttribute(this ComponentModel model, string name)
        {
            return model.Configuration?.Attributes[name];
        }
    }
}
