using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace SourceGeneration.Reflection;

internal class SourceTypeInfo
{
    public string BaseType;
    public string FullName;
    public string Name;
    public bool IsAbstract;
    public bool IsRecord;
    public bool IsReadOnly;
    public bool IsStatic;
    public bool IsStruct;
    public bool IsRefLikeType;
    public bool IsGenericType;
    public bool IsGenericTypeDefinition;
    public Accessibility Accessibility;

    public readonly List<SourceFieldInfo> Fields = [];
    public readonly List<SourcePropertyInfo> Properties = [];
    public readonly List<SourceMethodInfo> Methods = [];
    public readonly List<SourceConstructorInfo> Constructors = [];
}
