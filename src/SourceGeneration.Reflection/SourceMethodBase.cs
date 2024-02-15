namespace SourceGeneration.Reflection;

public abstract class SourceMethodBase : SourceMemberInfo
{
    public SourceAccessibility Accessibility { get; init; }
    public bool IsStatic { get; init; }
    public SourceParameterInfo[] Parameters { get; init; } = default!;
}
