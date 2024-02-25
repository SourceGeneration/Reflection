using Microsoft.CodeAnalysis;

namespace SourceGeneration.Reflection;

internal class SourceParameterInfo
{
    public string Name;
    public string ParameterType;
    public bool HasDefaultValue;
    public object DefaultValue;
    public bool IsTypeParameter;
    public bool HasNestedTypeParameter;
    public NullableAnnotation NullableAnnotation;
    public bool IsRef;
    public bool IsOut;

    public bool IsParameterTypeRefLike;
    public bool IsParameterTypePointer;
    public string DisplayType;
}
