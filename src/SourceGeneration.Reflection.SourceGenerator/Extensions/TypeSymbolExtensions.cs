using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

namespace SourceGeneration.Reflection.SourceGenerator;

internal static class TypeSymbolExtensions
{
    private static readonly SymbolDisplayFormat QualifiedNameFormat = new(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    private static readonly SymbolDisplayFormat NoGenericParameterTypeQualifiedNameFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        genericsOptions: SymbolDisplayGenericsOptions.None,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    public static int IndexOfTypeParameter(this IMethodSymbol method, string name)
    {
        for (int i = 0; i < method.TypeParameters.Length; i++)
        {
            if (method.TypeParameters[i].Name == name)
            {
                return i;
            }
        }
        return -1;
    }

    public static bool HasTypeParameter(this ITypeSymbol type)
    {
        if (type.Kind == SymbolKind.TypeParameter)
            return true;

        if (type is IArrayTypeSymbol array)
        {
            return HasTypeParameter(array.ElementType);
        }

        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            foreach (var argument in namedType.TypeArguments)
            {
                if (HasTypeParameter(argument))
                    return true;
            }
        }

        return false;
    }


    public static string ToReflectionDisplayString(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.Kind == SymbolKind.TypeParameter)
        {
            return typeSymbol.Name;
        }

        if (typeSymbol is IArrayTypeSymbol arrayType)
        {
            return $"{ToReflectionDisplayString(arrayType.ElementType)}[]";
        }

        if (typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            StringBuilder builder = new();

            builder.Append($"{namedType.ToDisplayString(NoGenericParameterTypeQualifiedNameFormat)}`{namedType.TypeParameters.Length}[");
            for (int i = 0; i < namedType.TypeArguments.Length; i++)
            {
                ITypeSymbol argument = namedType.TypeArguments[i];
                builder.Append(ToReflectionDisplayString(argument));
                if (i < namedType.TypeArguments.Length - 1)
                    builder.Append(',');
            }
            builder.Append(']');
            return builder.ToString();
        }

        return typeSymbol.ToDisplayString(QualifiedNameFormat);
    }

    public static bool IsCompliantGenericDictionaryInterface(this ITypeSymbol type)
    {
        if (type.TypeKind == TypeKind.Interface && type is INamedTypeSymbol namedType && namedType.IsGenericType && type.Name == "IDictionary")
            return true;

        return type.AllInterfaces.Any(x => x.IsGenericType && x.Name == "IDictionary");
    }

    public static bool IsCompliantGenericEnumerableInterface(this ITypeSymbol type)
    {
        if (type.TypeKind == TypeKind.Interface && type is INamedTypeSymbol namedType && namedType.IsGenericType && type.Name == "IEnumerable")
            return true;

        return type.AllInterfaces.Any(x => x.IsGenericType && x.Name == "IEnumerable");
    }

}
