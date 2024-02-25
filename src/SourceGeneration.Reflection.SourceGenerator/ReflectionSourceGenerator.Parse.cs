using Microsoft.CodeAnalysis;
using SourceGeneration.Reflection.SourceGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SourceGeneration.Reflection;

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
                yield return PaseEnum(typeSymbol);
            }
            else
            {
                yield return ParseType(typeSymbol);
            }

            if (typeSymbol.BaseType != null && typeSymbol.BaseType.ContainingAssembly.Equals(assemblySymbol, SymbolEqualityComparer.Default) && !typeSymbols.Contains(typeSymbol.BaseType, SymbolEqualityComparer.Default))
            {
                queue.Enqueue(typeSymbol.BaseType);
            }
        }
    }

    private static SourceTypeInfo PaseEnum(INamedTypeSymbol typeSymbol)
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
            typeInfo.Fields.Add(CreateField(field));
        }

        return typeInfo;
    }

    private static SourceTypeInfo ParseType(INamedTypeSymbol typeSymbol)
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

        foreach (var field in typeSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            typeInfo.Fields.Add(CreateField(field));
        }

        foreach (var property in typeSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            SourcePropertyInfo propertyInfo = new()
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

            typeInfo.Properties.Add(propertyInfo);
        }

        foreach (var constructor in typeSymbol.Constructors)
        {
            SourceConstructorInfo constructorInfo = new()
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
            typeInfo.Constructors.Add(constructorInfo);
        }

        foreach (var method in typeSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(x => x.MethodKind == MethodKind.Ordinary && !x.IsImplicitlyDeclared))
        {
            SourceMethodInfo methodInfo = new()
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
                    IsRef =  x.RefKind == RefKind.Ref,
                    IsOut = x.RefKind == RefKind.Out,
                    DefaultValue = x.HasExplicitDefaultValue ? x.ExplicitDefaultValue : null,
                    DisplayType = x.Type.ToReflectionDisplayString()
                }).ToList(),
            };

            typeInfo.Methods.Add(methodInfo);
        }

        return typeInfo;
    }


    private static SourceFieldInfo CreateField(IFieldSymbol field)
    {
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
            FieldType = field.Type.ToDisplayString(GlobalTypeDisplayFormat),
        };
    }

}
