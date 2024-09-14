using SourceGeneration.Reflection;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace SourceGeneration.AutoMapper;

public static class SourceAutoMapper
{
    [return: NotNullIfNotNull(nameof(source))]
    public static Dictionary<string, object?>? ToDictionary(this object source, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type, SourceAutoMapOptions? options = null)
    {
        var sourceType = SourceReflector.GetRequiredType(type, true);

        var sourceMembers = sourceType.GetProperties().Where(x => x.Accessibility == SourceAccessibility.Public && !x.IsStatic && x.CanRead).ToList();
        Dictionary<string, object?> dic = new(sourceMembers.Count);
        foreach (var member in sourceMembers)
        {
            var value = member.GetValue(source);
            if (value == null)
            {
                dic.Add(member.Name, null);
            }
            else if (member.PropertyType == typeof(string) || member.PropertyType.IsValueType || member.PropertyType.IsPrimitive)
            {
                dic.Add(member.Name, value);
            }
            else if (member.PropertyType.IsCompatibleDictionaryGenericInterface(out Type keyType, out Type valueType))
            {

            }
            else if (member.PropertyType.IsCompatibleEnumerableGenericInterface(out Type elementType))
            {
                dic.Add(member.Name, ToEnumerable((IEnumerable)value, elementType, options));
            }
            else
            {
                dic.Add(member.Name, ToDictionary(value, member.PropertyType, options));
            }
        }
        return dic;
    }

    //private static Dictionary<string, object?>? ToDictionary(this IEnumerable source, 
    //    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type keyType,
    //    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type valueType, 
    //    SourceAutoMapOptions? options = null)
    //{
    //    var keyTypeInfo = SourceReflector.GetRequiredType(keyType, true);
    //    var valeuTypeInfo = SourceReflector.GetRequiredType(valueType, true);
    //    Dictionary<string, object?> dic = [];
    //    foreach (var item in source)
    //    {
    //        dic.tryg
    //    }
    //}

    private static IEnumerable ToEnumerable(this IEnumerable source, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type, SourceAutoMapOptions? options = null)
    {
        if (type == typeof(string) || type.IsValueType || type.IsPrimitive)
        {
            return source;
        }
        else
        {
            List<object?> list = [];
            foreach (var item in source)
            {
                list.Add(ToDictionary(item, type));
            }
            return list;
        }
    }

    [return: NotNullIfNotNull(nameof(source))]
    public static TTarget? Map<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSource,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TTarget>
        (this TSource source, SourceAutoMapOptions? options = null) where TTarget : class, new()
    {
        if (source == null)
            return default;

        var sourceType = SourceReflector.GetRequiredType<TSource>(true);
        var targetType = SourceReflector.GetRequiredType<TTarget>(true);

        TTarget target = new();

        return (TTarget)Map(sourceType, targetType, source, target, options)!;
    }

    private static object? Map(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] SourceTypeInfo sourceType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] SourceTypeInfo targetType,
        object source,
        object target,
        SourceAutoMapOptions? options = null)
    {
        if (source == null)
            return default;

        options ??= SourceAutoMapOptions.Default;

        StringComparison stringComparison = options.PropertyNameCaseInsensitive ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;

        var sourceMembers = sourceType.GetProperties().Where(x => x.Accessibility == SourceAccessibility.Public && !x.IsStatic && x.CanRead).ToList();
        var targetMembers = targetType.GetProperties().Where(x => x.Accessibility == SourceAccessibility.Public && !x.IsStatic && x.CanWrite).ToList();

        foreach (var property in sourceMembers)
        {
            var targetProperty = targetMembers.FirstOrDefault(x => string.Equals(x.Name, property.Name, StringComparison.OrdinalIgnoreCase));

            if (targetProperty == null)
            {
                continue;
            }

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

                    var targetValue = Map(proeprtySourceType, propertyTargetType, propertySource, propertyTargetInstance, options);

                    targetProperty.SetValue(target, targetValue);
                }
            }
        }

        return target;
    }

}
