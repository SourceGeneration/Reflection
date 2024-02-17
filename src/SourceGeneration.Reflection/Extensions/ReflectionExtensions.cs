using System.ComponentModel;
using System.Reflection;

namespace SourceGeneration.Reflection;

internal static class ReflectionExtensions
{
    public const BindingFlags DeclaredOnlyLookup = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

#if NET5_0_OR_GREATER
    public const System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes DefaultAccessMembers =
        System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicParameterlessConstructor |
        System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors |
        System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicFields |
        System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties |
        System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicMethods |
        System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicConstructors |
        System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicFields |
        System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicProperties |
        System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicMethods;
#endif

    public static SourceAccessibility GetAccessibility(this MethodBase method)
    {
        if (method.IsPublic) return SourceAccessibility.Public;
        if (method.IsFamilyAndAssembly) return SourceAccessibility.ProtectedOrInternal;
        if (method.IsFamily) return SourceAccessibility.Protected;
        if (method.IsAssembly) return SourceAccessibility.Internal;
        return SourceAccessibility.Private;
    }

    public static SourceAccessibility GetAccessibility(this FieldInfo field)
    {
        if (field.IsPublic) return SourceAccessibility.Public;
        if (field.IsFamilyAndAssembly) return SourceAccessibility.ProtectedOrInternal;
        if (field.IsFamily) return SourceAccessibility.Protected;
        if (field.IsAssembly) return SourceAccessibility.Internal;
        return SourceAccessibility.Private;
    }

    public static SourceAccessibility GetAccessibility(this PropertyInfo property)
    {
        return property.CanRead ? property.GetMethod!.GetAccessibility() : property.SetMethod!.GetAccessibility();
    }


    private static readonly Type IsExternalInitType = typeof(System.Runtime.CompilerServices.IsExternalInit);
    public static bool IsInitOnly(this PropertyInfo propertyInfo)
    {
        MethodInfo? setMethod = propertyInfo.SetMethod;
        if (setMethod == null)
            return false;

        return setMethod.ReturnParameter.GetRequiredCustomModifiers().Contains(IsExternalInitType);
    }
}
