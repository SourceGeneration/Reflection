using Microsoft.CodeAnalysis;

namespace SourceGeneration.Reflection;

internal class SourceFieldInfo
{
    public string Name;
    public string FieldType;
    public bool IsGenericDictionaryType;
    public bool IsGenericEnumerableType;
    public bool IsRequired;
    public bool IsStatic;
    public bool IsReadOnly;
    public bool IsConst;
    public object ConstantValue;
    public Accessibility Accessibility;
    public NullableAnnotation NullableAnnotation;

    public string DefaultValueExpression;
}
