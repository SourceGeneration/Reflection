namespace SourceGeneration.Reflection;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
public class SourceReflectionAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class SourceReflectionTypeAttribute<T> : Attribute { }

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class SourceReflectionTypeAttribute(Type type) : Attribute
{
    public Type Type { get; } = type;
}