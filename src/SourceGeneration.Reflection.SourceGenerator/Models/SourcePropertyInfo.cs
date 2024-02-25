using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace SourceGeneration.Reflection;

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
    public bool IsIndexer;
    public bool IsInitOnly;
    public Accessibility Accessibility;
    public NullableAnnotation NullableAnnotation;
    public Accessibility GetMethodAccessibility;
    public Accessibility SetMethodAccessibility;
    public List<SourceParameterInfo> Parameters = [];
}
