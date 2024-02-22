namespace SourceGeneration.Reflection;

public static class SourceTypeInfoExtensions
{
    public static ISourceFieldOrPropertyInfo[] GetFieldsAndProperties(this SourceTypeInfo type) => type.GetFields().Cast<ISourceFieldOrPropertyInfo>().Union(type.GetProperties()).ToArray();

    public static ISourceFieldOrPropertyInfo? GetFieldOrProperty(this SourceTypeInfo type, string name) => (ISourceFieldOrPropertyInfo?)type.GetField(name) ?? type.GetProperty(name);

    public static ISourceFieldOrPropertyInfo GetRequriedFieldOrProperty(this SourceTypeInfo type, string name)
    {
        return (ISourceFieldOrPropertyInfo?)type.GetField(name) ?? type.GetProperty(name) ?? throw new KeyNotFoundException($"Field or property '{name}' not found.");
    }

    public static SourceFieldInfo GetRequriedField(this SourceTypeInfo sourceTypeInfo, string name)
    {
        return sourceTypeInfo.GetField(name) ?? throw new KeyNotFoundException($"Field '{name}' not found.");
    }

    public static SourcePropertyInfo GetRequriedProperty(this SourceTypeInfo sourceTypeInfo, string name)
    {
        return sourceTypeInfo.GetProperty(name) ?? throw new KeyNotFoundException($"Property '{name}' not found.");
    }
}