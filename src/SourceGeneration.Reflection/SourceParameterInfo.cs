using System.Reflection;

namespace SourceGeneration.Reflection;

public class SourceParameterInfo
{
    public string Name { get; init; } = default!;

#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(ReflectionExtensions.DefaultAccessMembers)]
#endif
    public Type ParameterType { get; init; } = default!;
    public bool HasDefaultValue { get; init; }
    public object? DefaultValue { get; init; } = default!;
    public SourceNullableAnnotation NullableAnnotation { get; init; }

    //private Func<ParameterInfo>? _parameterInfoAccess;
    //public ParameterInfo ParameterInfo
    //{
    //    get => 
    //}

}