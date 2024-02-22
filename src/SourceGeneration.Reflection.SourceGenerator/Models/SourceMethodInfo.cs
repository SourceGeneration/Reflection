using Microsoft.CodeAnalysis;

namespace SourceGeneration.Reflection;

internal class SourceMethodInfo : SourceMethodBase
{
    public string ReturnType;
    public bool IsVirtual;
    public bool IsOverride;
    public bool IsAbstract;
    public NullableAnnotation ReturnNullableAnnotation;

}
