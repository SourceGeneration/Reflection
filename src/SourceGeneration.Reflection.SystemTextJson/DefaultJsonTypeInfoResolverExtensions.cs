using SourceGeneration.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace System.Text.Json;

public static class JsonTypeInfoResolverExtensions
{
    public static IJsonTypeInfoResolver WithSourceReflection(this IJsonTypeInfoResolver resolver)
    {
#if NET7_0
        ((DefaultJsonTypeInfoResolver)resolver).Modifiers.Add(Modify);
        return resolver;
#else
        return resolver.WithAddedModifier(Modify);
#endif
    }

    private static void Modify(JsonTypeInfo typeInfo)
    {
        var metadata = SourceReflector.GetType(typeInfo.Type);
        if (metadata == null)
            return;

        if (metadata.DeclaredConstructors.Any(x => x.Parameters.Length == 0))
        {
            var ctror = metadata.DeclaredConstructors.FirstOrDefault(x => x.Parameters.Length == 0);
            if (ctror != null)
            {
                typeInfo.CreateObject = () => ctror.Invoke(null);
            }
        }

        foreach (var member in typeInfo.Properties)
        {
            var metaProperty = metadata.GetFieldOrProperty(member.Name);
            if (metaProperty != null)
            {
                if (metaProperty is SourceFieldInfo field)
                {
                    member.Get = metaProperty.GetValue;
                    if (!field.IsReadOnly)
                        member.Set = metaProperty.SetValue;
                }
                else if (metaProperty is SourcePropertyInfo property)
                {
                    if (property.CanRead)
                        member.Get = metaProperty.GetValue;

                    if (property.CanWrite)
                        member.Set = metaProperty.SetValue;
                }
            }
        }
    }
}