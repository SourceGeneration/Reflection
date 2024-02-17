using SourceGeneration.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace SourceGeneration.AutoMapper;

public static class SourceAutoMapper
{
    [return: NotNullIfNotNull(nameof(source))]
    public static TTarget? Map<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSource,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TTarget>
        (this TSource source, bool propertyNameCaseInsensitive = true) where TTarget : class, new()
    {
        if (source == null)
            return default;

        var sourceType = SourceReflector.GetType<TSource>(true)!;
        var targetType = SourceReflector.GetType<TTarget>(true)!;

        TTarget target = new();

        StringComparison stringComparison = propertyNameCaseInsensitive ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;

        foreach (var property in sourceType.GetProperties().Where(x => x.Accessibility == SourceAccessibility.Public && x.CanRead))
        {
            var targetProperty = targetType.GetProperties().FirstOrDefault(x 
                => x.Accessibility == SourceAccessibility.Public 
                && string.Equals(x.Name, property.Name, StringComparison.OrdinalIgnoreCase) 
                && x.PropertyType.IsAssignableFrom(property.PropertyType));

            if (targetProperty != null && targetProperty.CanWrite && !targetProperty.IsInitOnly)
            {
                targetProperty.SetValue(target, property.GetValue(source));
            }
        }

        return target;
    }
}
