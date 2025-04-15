using Microsoft.CodeAnalysis;
using SourceGeneration.Reflection.SourceGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SourceGeneration.Reflection;

#pragma warning disable IDE0305

public partial class ReflectionSourceGenerator
{
    private static readonly SymbolDisplayFormat GlobalTypeDisplayFormat = new SymbolDisplayFormat(
                SymbolDisplayGlobalNamespaceStyle.Included, SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces)
                .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters);
    //.AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.UseSpecialTypes/* | SymbolDisplayMiscellaneousOptions.ExpandNullable*/);

    private static readonly SymbolDisplayFormat QualifiedNameFormat = new(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    private static IEnumerable<SourceTypeInfo> Parse(IAssemblySymbol assemblySymbol, IEnumerable<INamedTypeSymbol> typeSymbols, CancellationToken cancellationToken)
    {
        Queue<INamedTypeSymbol> queue = new(typeSymbols);
        while (queue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var typeSymbol = queue.Dequeue();

            if (typeSymbol.DeclaredAccessibility == Accessibility.Private ||
                typeSymbol.DeclaredAccessibility == Accessibility.Protected)
                continue;

            if (typeSymbol.TypeKind == TypeKind.Enum)
            {
                yield return PaseEnumType(typeSymbol, cancellationToken);
            }
            else
            {
                yield return ParseClassType(typeSymbol, cancellationToken);
            }

            if (typeSymbol.BaseType != null && typeSymbol.BaseType.ContainingAssembly.Equals(assemblySymbol, SymbolEqualityComparer.Default) && !typeSymbols.Contains(typeSymbol.BaseType, SymbolEqualityComparer.Default))
            {
                if (!queue.Contains(typeSymbol.BaseType))
                {
                    queue.Enqueue(typeSymbol.BaseType);
                }
            }
        }
    }

    private static SourceTypeInfo PaseEnumType(INamedTypeSymbol typeSymbol, CancellationToken cancellationToken)
    {
        var fullname = typeSymbol.ToDisplayString(GlobalTypeDisplayFormat);
        SourceTypeInfo typeInfo = new()
        {
            Name = typeSymbol.Name,
            BaseType = typeSymbol.BaseType.ToDisplayString(GlobalTypeDisplayFormat),
            EnumUnderlyingType = typeSymbol.EnumUnderlyingType.ToDisplayString(GlobalTypeDisplayFormat),
            FullName = typeSymbol.ToDisplayString(QualifiedNameFormat),
            FullGlobalName = fullname,
            IsEnum = true,
            Accessibility = typeSymbol.DeclaredAccessibility,
        };

        foreach (var field in typeSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            typeInfo.Fields.Add(CreateField(field));
        }

        return typeInfo;
    }

    private static SourceTypeInfo ParseClassType(INamedTypeSymbol typeSymbol, CancellationToken cancellationToken)
    {
        string fullname;
        bool isGenericTypeDefinition;
        if (typeSymbol.IsGenericType && typeSymbol.TypeArguments[0].Kind == SymbolKind.TypeParameter)
        {
            isGenericTypeDefinition = true;
            fullname = typeSymbol.ToDisplayString(QualifiedNameFormat) + $"<{new string(',', typeSymbol.TypeParameters.Length - 1)}>";
        }
        else
        {
            isGenericTypeDefinition = false;
            fullname = typeSymbol.ToDisplayString(GlobalTypeDisplayFormat);
        }

        SourceTypeInfo typeInfo = new()
        {
            Name = typeSymbol.Name,
            BaseType = typeSymbol.BaseType?.ToDisplayString(GlobalTypeDisplayFormat),
            FullName = typeSymbol.ToDisplayString(QualifiedNameFormat),
            FullGlobalName = fullname,
            IsAbstract = typeSymbol.IsAbstract,
            IsStatic = typeSymbol.IsStatic,
            IsRecord = typeSymbol.IsRecord,
            IsReadOnly = typeSymbol.IsReadOnly,
            IsStruct = typeSymbol.TypeKind == TypeKind.Struct,
            IsRefLikeType = typeSymbol.IsRefLikeType,
            IsGenericType = typeSymbol.IsGenericType,
            IsGenericTypeDefinition = isGenericTypeDefinition,
            Accessibility = typeSymbol.DeclaredAccessibility,
        };

        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            foreach (var member in baseType.GetMembers().Where(x => !x.IsAbstract && !x.IsOverride))
            {
                if (member is IPropertySymbol property)
                {
                    if (property.IsRequired)
                    {
                        typeInfo.HasBaseRequiredMembers = true;
                        break;
                        //var value = property.GetInitializeValue(cancellationToken);
                        //typeInfo.BaseRequiredMembers.Add(property.Name, value);
                    }
                }
                else if (member is IFieldSymbol field)
                {
                    if (field.IsRequired)
                    {
                        typeInfo.HasBaseRequiredMembers = true;
                        break;
                        //var value = field.GetInitializeValue(cancellationToken);
                        //typeInfo.BaseRequiredMembers.Add(field.Name, value);
                    }
                }
            }
            baseType = baseType.BaseType;
        }


        var members = typeSymbol.GetMembers();

        foreach (var field in members.OfType<IFieldSymbol>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            typeInfo.Fields.Add(CreateField(field));
        }

        foreach (var property in members.OfType<IPropertySymbol>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            typeInfo.Properties.Add(CreateProperty(property));
        }

        foreach (var constructor in typeSymbol.Constructors)
        {
            cancellationToken.ThrowIfCancellationRequested();
            typeInfo.Constructors.Add(CreateConstructor(constructor));
        }

        foreach (var method in members.OfType<IMethodSymbol>().Where(x => x.MethodKind == MethodKind.Ordinary && !x.IsImplicitlyDeclared))
        {
            cancellationToken.ThrowIfCancellationRequested();
            typeInfo.Methods.Add(CreateMethod(method));
        }

        return typeInfo;
    }

    private static SourceMethodInfo CreateMethod(IMethodSymbol method)
    {
        return new()
        {
            Name = method.Name,
            Accessibility = method.DeclaredAccessibility,
            IsOverride = method.IsOverride,
            IsVirtual = method.IsVirtual,
            IsAbstract = method.IsAbstract,
            IsStatic = method.IsStatic,
            IsGenericMethod = method.IsGenericMethod,
            ReturnType = method.ReturnsVoid ? "void" : method.ReturnType.HasTypeParameter() ? null : method.ReturnType.ToDisplayString(GlobalTypeDisplayFormat),
            ReturnNullableAnnotation = method.ReturnNullableAnnotation,
            TypeParameters = method.TypeParameters.Select(x => new SourceTypeParameterInfo
            {
                Name = x.Name,
                HasUnmanagedTypeConstraint = x.HasUnmanagedTypeConstraint,
                HasValueTypeConstraint = x.HasValueTypeConstraint,
                HasTypeParameterInConstraintTypes = x.ConstraintTypes.Any(x => x.HasTypeParameter()),
                ConstraintTypes = x.ConstraintTypes.Select(x => x.ToDisplayString(GlobalTypeDisplayFormat)).ToArray(),
            }).ToArray(),
            Parameters = method.Parameters.Select(x => new SourceParameterInfo
            {
                Name = x.Name,
                ParameterType = x.Type.ToDisplayString(GlobalTypeDisplayFormat),
                IsParameterTypeRefLike = x.Type.IsRefLikeType,
                IsParameterTypePointer = x.Type.Kind == SymbolKind.PointerType,
                NullableAnnotation = x.NullableAnnotation,
                HasDefaultValue = x.HasExplicitDefaultValue,
                IsTypeParameter = x.Type.Kind == SymbolKind.TypeParameter,
                HasNestedTypeParameter = x.Type.HasTypeParameter(),
                IsRef = x.RefKind == RefKind.Ref,
                IsOut = x.RefKind == RefKind.Out,
                DefaultValue = x.HasExplicitDefaultValue ? x.ExplicitDefaultValue : null,
                DisplayType = x.Type.ToReflectionDisplayString()
            }).ToList(),
        };
    }

    private static SourceConstructorInfo CreateConstructor(IMethodSymbol constructor)
    {
        return new()
        {
            Name = constructor.Name,
            IsStatic = constructor.IsStatic,
            Accessibility = constructor.DeclaredAccessibility,
            Parameters = constructor.Parameters.Select(x => new SourceParameterInfo
            {
                Name = x.Name,
                ParameterType = x.Type.ToDisplayString(GlobalTypeDisplayFormat),
                IsParameterTypeRefLike = x.Type.IsRefLikeType,
                IsParameterTypePointer = x.Type.Kind == SymbolKind.PointerType,
                NullableAnnotation = x.NullableAnnotation,
                HasDefaultValue = x.HasExplicitDefaultValue,
                HasNestedTypeParameter = x.Type.TypeKind == TypeKind.TypeParameter,
                DefaultValue = x.HasExplicitDefaultValue ? x.ExplicitDefaultValue : null,
            }).ToList(),
        };
    }

    private static SourcePropertyInfo CreateProperty(IPropertySymbol property)
    {
        //string defaultValueExpression = property.IsRequired ? property.GetInitializeValue(cancellationToken) :null;

        return new()
        {
            Name = property.Name,
            Accessibility = property.DeclaredAccessibility,
            NullableAnnotation = property.NullableAnnotation,
            IsVirtual = property.IsVirtual,
            IsRequired = property.IsRequired,
            IsAbstract = property.IsAbstract,
            IsStatic = property.IsStatic,
            IsIndexer = property.IsIndexer,
            CanRead = property.GetMethod != null,
            CanWrite = property.SetMethod != null,
            IsInitOnly = property.SetMethod?.IsInitOnly == true,
            GetMethodAccessibility = property.GetMethod?.DeclaredAccessibility ?? Accessibility.NotApplicable,
            SetMethodAccessibility = property.SetMethod?.DeclaredAccessibility ?? Accessibility.NotApplicable,

            IsGenericDictionaryType = property.Type.IsCompliantGenericDictionaryInterface(),
            IsGenericEnumerableType = property.Type.IsCompliantGenericEnumerableInterface(),

            //DefaultValueExpression = defaultValueExpression,

            PropertyType = property.Type.ToDisplayString(GlobalTypeDisplayFormat),
            Parameters = property.Parameters.Select(x => new SourceParameterInfo
            {
                Name = x.Name,
                ParameterType = x.Type.ToDisplayString(GlobalTypeDisplayFormat),
                NullableAnnotation = x.NullableAnnotation,
                HasDefaultValue = x.HasExplicitDefaultValue,
                HasNestedTypeParameter = x.Type.TypeKind == TypeKind.TypeParameter,
                DefaultValue = x.HasExplicitDefaultValue ? x.ExplicitDefaultValue : null,
            }).ToList(),
        };
    }

    private static SourceFieldInfo CreateField(IFieldSymbol field)
    {
        //string defaultValueExpression = field.IsRequired ? field.GetInitializeValue(cancellationToken) : null;

        return new()
        {
            Name = field.Name,
            Accessibility = field.DeclaredAccessibility,
            NullableAnnotation = field.NullableAnnotation,
            IsRequired = field.IsRequired,
            IsConst = field.IsConst,
            ConstantValue = field.ConstantValue,
            IsStatic = field.IsStatic,
            IsReadOnly = field.IsReadOnly,

            IsGenericDictionaryType = field.Type.IsCompliantGenericDictionaryInterface(),
            IsGenericEnumerableType = field.Type.IsCompliantGenericEnumerableInterface(),

            FieldType = field.Type.ToDisplayString(GlobalTypeDisplayFormat),

            //DefaultValueExpression = defaultValueExpression,
        };
    }


}
