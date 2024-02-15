using System.Reflection;

namespace SourceGeneration.Reflection;

public class SourcePropertyInfo(Func<PropertyInfo> propertyInfoAccess) : SourceMemberInfo, ISourceFieldOrPropertyInfo
{
    private Func<object, object?>? _getMethod;
    private Action<object, object?>? _setMethod;
    private PropertyInfo? _propertyInfo;

    public PropertyInfo PropertyInfo => _propertyInfo ??= propertyInfoAccess();

    public SourceAccessibility Accessibility { get; init; }

#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(ReflectionExtensions.DefaultAccessMembers)]
#endif
    public Type PropertyType { get; init; } = default!;

    public bool IsVirtual { get; init; }
    public bool IsRequired { get; init; }
    public bool IsAbstract { get; init; }
    public bool IsStatic { get; init; }
    public bool IsInitOnly { get; init; }
    public SourceNullableAnnotation NullableAnnotation { get; init; }

    public bool CanWrite { get; init; }
    public bool CanRead { get; init; }

    public Func<object, object?> GetValue
    {
        get => _getMethod ?? PropertyInfo.GetValue;
        init => _getMethod = value; 
    }

    public Action<object, object?> SetValue
    {
        get => _setMethod ?? PropertyInfo.SetValue;
        init => _setMethod = value;
    }

#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(ReflectionExtensions.DefaultAccessMembers)]
#endif
    Type ISourceFieldOrPropertyInfo.MemberType => PropertyType;

    MemberInfo ISourceFieldOrPropertyInfo.MemberInfo => PropertyInfo;
    Func<object?, object?> ISourceFieldOrPropertyInfo.GetValue => GetValue!;
    Action<object?, object?> ISourceFieldOrPropertyInfo.SetValue => SetValue!;

}
