using System.Reflection;

namespace SourceGeneration.Reflection;

public interface ISourceFieldOrPropertyInfo
{
    string Name { get; }
    SourceAccessibility Accessibility { get; }

#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(ReflectionExtensions.DefaultAccessMembers)]
#endif
    Type MemberType { get; }

    MemberInfo MemberInfo { get; }

    bool IsStatic { get; }
    bool IsRequired { get; }

    Func<object?, object?> GetValue { get; }
    Action<object?, object?> SetValue { get; }
}
