namespace Castle.Facilities.ServiceFabricIntegration
{
    using System;
    using System.Linq;
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

        public static DependencyModel GetDependencyFor(this ComponentModel model, Type type)
        {
            return model.Dependencies.FirstOrDefault(dependency => dependency.TargetItemType.IsAssignableFrom(type));
        }

        public static bool TryGetDependencyType<TDependency>(this ComponentModel model, out Type dependencyType)
        {
            var dependency = model.GetDependencyFor(typeof(TDependency));
            if (dependency == null)
            {
                dependencyType = null;
                return false;
            }

            dependencyType = dependency.TargetItemType;
            return true;
        }

        public static T GetProperty<T>(this IHandler handler, object key)
        {
            return handler.ComponentModel.GetProperty<T>(key);
        }

        public static T GetProperty<T>(this ComponentModel model, object key)
        {
            return (T)model.ExtendedProperties[key];
        }

        public static void SetProperty(this ComponentModel model, object key, object value)
        {
            model.ExtendedProperties[key] = value;
        }

        public static void SetProperty<T>(this ComponentModel model, T value)
        {
            model.SetProperty(typeof(T), value);
        }

        public static string GetAttribute(this ComponentModel model, string name)
        {
            return model.Configuration?.Attributes[name];
        }
    }
}
