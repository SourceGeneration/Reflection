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

        var sourceType = SourceReflector.GetRequiredType<TSource>(true);
        var targetType = SourceReflector.GetRequiredType<TTarget>(true);

        TTarget target = new();

        return (TTarget)Map(sourceType, targetType, source, target, propertyNameCaseInsensitive)!;
    }

    private static object? Map(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] SourceTypeInfo sourceType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] SourceTypeInfo targetType,
        object source,
        object target,
        bool propertyNameCaseInsensitive = true)
    {
        if (source == null)
            return default;

        StringComparison stringComparison = propertyNameCaseInsensitive ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;

        foreach (var property in sourceType.GetProperties().Where(x => x.Accessibility == SourceAccessibility.Public && x.CanRead))
        {
            var targetProperty = targetType.GetProperties().FirstOrDefault(x
                => x.Accessibility == SourceAccessibility.Public
                && !x.IsStatic
                && string.Equals(x.Name, property.Name, StringComparison.OrdinalIgnoreCase));

            if (targetProperty != null && targetProperty.CanWrite)
            {
                if (targetProperty.PropertyType.IsAssignableFrom(property.PropertyType))
                {
                    targetProperty.SetValue(target, property.GetValue(source));
                }
                else if (targetProperty.PropertyType.IsValueType || property.PropertyType.IsValueType)
                {
                    continue;
                }
                else
                {
                    var propertySource = property.GetValue(source);
                    if (propertySource == null)
                    {
                        targetProperty.SetValue(target, null);
                    }
                    else
                    {
                        var proeprtySourceType = SourceReflector.GetRequiredType(property.PropertyType, true);
                        var propertyTargetType = SourceReflector.GetRequiredType(targetProperty.PropertyType, true);
                        var propertyTargetInstance = SourceReflector.CreateInstance(targetProperty.PropertyType, []);

                        var targetValue = Map(proeprtySourceType, propertyTargetType, propertySource, propertyTargetInstance);

                        targetProperty.SetValue(target, targetValue);
                    }
                }
            }
        }

        return target;
    }

}
