using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace SourceGeneration.Reflection;

internal class SourceConstructorInfo : SourceMethodBase
{
}
internal class SourceFieldInfo
{
    public string Name;
    public string FieldType;
    public bool IsRequired;
    public bool IsStatic;
    public bool IsReadOnly;
    public bool IsConst;
    public object ConstantValue;
    public Accessibility Accessibility;
    public NullableAnnotation NullableAnnotation;
}

internal class SourcePropertyInfo
{
    public string Name;
    public string PropertyType;
    public bool IsVirtual;
    public bool IsRequired;
    public bool IsAbstract;
    public bool IsStatic;
    public bool CanWrite;
    public bool CanRead;
    public bool IsInitOnly;
    public Accessibility Accessibility;
    public NullableAnnotation NullableAnnotation;
}

internal class SourceMethodBase
{
    public string Name;
    public bool IsStatic;
    public List<SourceParameterInfo> Parameters = [];
    public Accessibility Accessibility;
}

internal class SourceMethodInfo : SourceMethodBase
{
    public string ReturnType;
    public bool IsVirtual;
    public bool IsOverride;
    public bool IsAbstract;
    public NullableAnnotation ReturnNullableAnnotation;

}


internal class SourceParameterInfo
{
    public string Name;
    public string ParameterType;
    public bool HasDefaultValue;
    public object DefaultValue;
    public NullableAnnotation NullableAnnotation;
}