using System.Reflection;

namespace SourceGeneration.Reflection;

public class SourceFieldInfo(Func<FieldInfo> fieldInfoAccess) : SourceMemberInfo, ISourceFieldOrPropertyInfo
{
    private Func<object?, object?>? _getMethod;
    private Action<object?, object?>? _setMethod;
    private FieldInfo? _fieldInfo;

    public FieldInfo FieldInfo => _fieldInfo ??= fieldInfoAccess();

    public SourceAccessibility Accessibility { get; init; }

#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(ReflectionExtensions.DefaultAccessMembers)]
#endif
    public Type FieldType { get; init; } = default!;
    public bool IsRequired { get; init; }
    public bool IsStatic { get; init; }
    public bool IsReadOnly { get; init; }
    public bool IsConst { get; init; }

    public bool IsGenericDictionaryType { get; init; }
    public bool IsGenericEnumerableType { get; init; }

    public SourceNullableAnnotation NullableAnnotation { get; init; }

    public Func<object?, object?> GetValue
    {
        get => _getMethod ?? FieldInfo.GetValue;
        init => _getMethod = value;
    }

    public Action<object?, object?> SetValue
    {
        get => _setMethod ?? FieldInfo.SetValue;
        init => _setMethod = value;
    }

#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(ReflectionExtensions.DefaultAccessMembers)]
#endif
    Type ISourceFieldOrPropertyInfo.MemberType => FieldType;

    MemberInfo ISourceFieldOrPropertyInfo.MemberInfo => FieldInfo;
}
