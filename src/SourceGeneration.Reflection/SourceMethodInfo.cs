using System.Reflection;

namespace SourceGeneration.Reflection;

public class SourceMethodInfo(Func<MethodInfo> getMethodInfo) : SourceMethodBase
{
    private MethodInfo? _methodInfo;
    private Func<object?, object?[]?, object?>? _invoke;

    public MethodInfo MethodInfo => _methodInfo ??= getMethodInfo();

#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(ReflectionExtensions.DefaultAccessMembers)]
#endif

    private Type? _returnType;

    public Type ReturnType
    {
        get => _returnType ??= MethodInfo.ReturnType;
        init => _returnType = value;
    }

    public SourceNullableAnnotation ReturnNullableAnnotation { get; init; } = default!;

    public Func<object?, object?[]?, object?> Invoke
    {
        get => _invoke ?? MethodInfo.Invoke;
        init => _invoke = value;
    }
}
