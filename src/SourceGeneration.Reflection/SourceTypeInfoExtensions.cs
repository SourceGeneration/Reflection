namespace SourceGeneration.Reflection;

public static class SourceTypeInfoExtensions
{
    public static ISourceFieldOrPropertyInfo? GetFieldOrPropertyInfo(this SourceTypeInfo type, string name) => (ISourceFieldOrPropertyInfo?)type.GetField(name) ?? type.GetProperty(name);
    public static ISourceFieldOrPropertyInfo[] GetFieldOrPropertyInfos(this SourceTypeInfo type) => type.GetFields().Cast<ISourceFieldOrPropertyInfo>().Union(type.GetProperties()).ToArray();
}