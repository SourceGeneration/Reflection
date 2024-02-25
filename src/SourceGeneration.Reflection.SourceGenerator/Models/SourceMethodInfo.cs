using Microsoft.CodeAnalysis;
using System.Linq;

namespace SourceGeneration.Reflection;

internal class SourceMethodInfo : SourceMethodBase
{
    public string ReturnType;
    public bool IsVirtual;
    public bool IsOverride;
    public bool IsAbstract;
    public bool IsGenericMethod;
    public NullableAnnotation ReturnNullableAnnotation;

    public SourceTypeParameterInfo[] TypeParameters;

    public bool CanInvoke()
    {
        if (Accessibility == Accessibility.Private || Accessibility == Accessibility.Protected)
            return false;

        if (IsGenericMethod)
        {
            if (TypeParameters.Any(x => x.HasUnmanagedTypeConstraint || x.ConstraintTypes.Length > 1))
                return false;

            if (Parameters.Any(x => !x.IsTypeParameter && x.HasNestedTypeParameter))
                return false;

            if (!TypeParameters.All(t => Parameters.Any(x => x.ParameterType == t.Name)))
                return false;
        }

        return true;
    }

    public int IndexOfTypeParameter(string type)
    {
        for (int i = 0; i < TypeParameters.Length; i++)
        {
            if (TypeParameters[i].Name == type)
                return i;
        }
        return -1;
    }
}
