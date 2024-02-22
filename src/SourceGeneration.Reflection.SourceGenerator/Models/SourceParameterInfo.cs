using Microsoft.CodeAnalysis;

namespace SourceGeneration.Reflection;

internal class SourceParameterInfo
{
    public string Name;
    public string ParameterType;
    public bool HasDefaultValue;
    public object DefaultValue;
    public NullableAnnotation NullableAnnotation;
}