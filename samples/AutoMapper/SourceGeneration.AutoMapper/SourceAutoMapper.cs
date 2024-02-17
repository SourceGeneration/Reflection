using SourceGeneration.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace SourceGeneration.AutoMapper;

public static class SourceAutoMapper
{
    [return: NotNullIfNotNull(nameof(source))]
    public static TTarget? Map<TSource, TTarget>(this TSource source) where TTarget : class, new()
    {
        if (source == null)
            return default;

        var sourceType = SourceReflector.GetType<TSource>();
        var targetType = SourceReflector.GetType<TTarget>();

        TTarget target = new();

        foreach (var property in sourceType!.GetProperties().Where(x => x.CanRead))
        {
            var targetProperty = targetType!.GetProperty(property.Name);

            if (targetProperty != null && targetProperty.CanWrite && !targetProperty.IsInitOnly)
            {
                targetProperty.SetValue(target, property.GetValue(source));
            }
        }

        return target;
    }
}
