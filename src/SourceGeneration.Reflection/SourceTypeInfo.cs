using System.Reflection;

namespace SourceGeneration.Reflection;

public class SourceTypeInfo : SourceMemberInfo
{
    public SourceAccessibility Accessibility { get; init; }

#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(ReflectionExtensions.DefaultAccessMembers)]
#endif
    public Type Type { get; init; } = default!;

#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(ReflectionExtensions.DefaultAccessMembers)]
#endif
    public Type? BaseType { get; init; }

    public Type? EnumUnderlyingType { get; init; }

    public bool IsRecord { get; init; }
    public bool IsEnum { get; init; }
    public bool IsStatic { get; init; }
    public bool IsStruct { get; init; }
    public bool IsReadOnly { get; init; }
    public bool IsReflected { get; init; }

    public SourcePropertyInfo[] DeclaredProperties { get; init; } = default!;
    public SourceMethodInfo[] DeclaredMethods { get; init; } = default!;
    public SourceConstructorInfo[] DeclaredConstructors { get; init; } = default!;
    public SourceFieldInfo[] DeclaredFields { get; init; } = default!;

    public SourcePropertyInfo[] GetProperties() => EnumerableInherit(this).SelectMany(x => x.DeclaredProperties).ToArray();
    public SourceFieldInfo[] GetFields() => EnumerableInherit(this).SelectMany(x => x.DeclaredFields).ToArray();
    public SourceConstructorInfo[] GetConstructors() => EnumerableInherit(this).SelectMany(x => x.DeclaredConstructors).ToArray();
    public SourceMethodInfo[] GetMethods() => EnumerableInherit(this).SelectMany(x => x.DeclaredMethods).ToArray();

    private static IEnumerable<SourceTypeInfo> EnumerableInherit(SourceTypeInfo type)
    {
        yield return type;
        for (; ; )
        {
            if (type.BaseType == null)
                break;

            type = SourceReflector.GetType(type.BaseType, type.IsReflected)!;
            if (type == null)
                break;
            yield return type;
        }
    }

    public SourcePropertyInfo? GetProperty(string name) => GetProperties().FirstOrDefault(x => x.Name == name);

    public SourceFieldInfo? GetField(string name) => GetFields().FirstOrDefault(x => x.Name == name);

    public SourceMethodInfo? GetMethod(string name)
    {
        var methods = GetMethods().Where(x => x.Name == name).ToList();
        if (methods.Count == 1)
            return methods[0];

        return null;
    }

    public SourceMethodInfo? GetMethod(string name, Type[] types)
    {
        return GetMethods().FirstOrDefault(x => x.Name == name && x.Parameters.Select(p => p.ParameterType).SequenceEqual(types));
    }

    public SourceConstructorInfo? GetConstructor(Type[] types)
    {
        return GetConstructors().FirstOrDefault(x => x.Parameters.Select(p => p.ParameterType).SequenceEqual(types));
    }

}
