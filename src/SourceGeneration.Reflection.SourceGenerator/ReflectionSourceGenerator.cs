using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SourceGeneration.Reflection;

[Generator(LanguageNames.CSharp)]
public partial class ReflectionSourceGenerator : IIncrementalGenerator
{
    private static readonly SymbolDisplayFormat GlobalTypeDisplayFormat = new SymbolDisplayFormat(SymbolDisplayGlobalNamespaceStyle.Included, SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces)
                .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters)
                .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.UseSpecialTypes/* | SymbolDisplayMiscellaneousOptions.ExpandNullable*/);

    private static readonly SymbolDisplayFormat QualifiedNameFormat = new(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    private const string SourceReflectionAttributeName = "SourceGeneration.Reflection.SourceReflectionAttribute";
    private static string[] CustomSourceReflectionAttributes = [];
    private static string[] SourceReflectionTypes = [];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterImplementationSourceOutput(context.AdditionalTextsProvider.Collect().Combine(context.AnalyzerConfigOptionsProvider),
            (context, source) =>
            {
                List<string> types = [];

                foreach (var additional in source.Left)
                {
                    if (Path.GetFileName(additional.Path) == "SourceReflection.txt")
                    {
                        var text = additional.GetText(context.CancellationToken);
                        foreach (var line in text.Lines)
                        {
                            if (line.Span.IsEmpty)
                                continue;

                            var lineText = text.GetSubText(line.Span).ToString();

                            if (!lineText.StartsWith("@type "))
                                continue;

                            var type = lineText.Substring(6).Trim();
                            if (type.Length == 0)
                                continue;

                            if (!types.Contains(type))
                                types.Add(type);
                        }
                    }
                }

                SourceReflectionTypes = [.. types];
            });

        context.RegisterImplementationSourceOutput(context.AnalyzerConfigOptionsProvider,
                (context, provider) =>
                {
                    CustomSourceReflectionAttributes = provider.GlobalOptions.Keys
                        .Where(x => x.StartsWith("build_property.") && x.EndsWith("sourcereflectionattribute", StringComparison.OrdinalIgnoreCase))
                        .Select(x =>
                        {
                            provider.GlobalOptions.TryGetValue(x, out var attribute);
                            return attribute;
                        })
                        .Where(x => x != null && x.EndsWith("Attribute"))
                        .Distinct()
                        .ToArray();
                });

        var results = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) =>
            {
                if (node is not BaseTypeDeclarationSyntax baseType)
                    return false;

                if (SourceReflectionTypes.Length > 0)
                {
                    var typeName = baseType.Identifier.ToFullString().Trim();

                    if (SourceReflectionTypes.Any(x => MatchFullName(x, typeName)))
                        return true;
                }

                if (baseType is TypeDeclarationSyntax type)
                {
                    if (type.TypeParameterList != null || type.TypeParameterList?.Parameters.Count > 0)
                        return false;
                }

                return baseType.AttributeLists.Any(x => x.Attributes.Any(x =>
                {
                    var name = x.Name.ToFullString().Trim();
                    if (!name.EndsWith("Attribute"))
                        name += "Attribute";

                    if (MatchFullName(SourceReflectionAttributeName, name))
                        return true;

                    if (CustomSourceReflectionAttributes.Any(x => MatchFullName(x, name)))
                        return true;

                    return false;
                }));
            },
            static (context, cancellationToken) =>
            {
                return (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken);
            });

        context.RegisterSourceOutput(results.Collect().Combine(context.CompilationProvider), static (sourceContext, source) =>
        {
            var symbols = source.Left;
            if (symbols.Length == 0)
                return;

            CancellationToken cancellationToken = sourceContext.CancellationToken;
            var assembly = source.Right.Assembly;

            var types = Parse(assembly, symbols, cancellationToken).ToList();
            if (types.Count == 0)
                return;

            var code = Emit(types, cancellationToken);

            sourceContext.AddSource("__SourceReflection.ModuleInitializerAttribute.g.cs", @"#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    internal sealed class ModuleInitializerAttribute : System.Attribute { }
}
#endif");

            sourceContext.AddSource("__SourceReflection.SourceReflectorInitializer.g.cs", code);
        });
    }

    private static bool MatchFullName(string fullname, string name)
    {
        if (fullname.EndsWith(name))
        {
            if (fullname.Length == name.Length)
                return true;

            if (fullname.Length > name.Length && fullname[fullname.Length - name.Length - 1] == '.')
                return true;
        }
        return false;
    }

    private static IEnumerable<SourceTypeInfo> Parse(IAssemblySymbol assemblySymbol, ImmutableArray<INamedTypeSymbol> typeSymbols, CancellationToken cancellationToken)
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

            if (typeSymbol.BaseType.ContainingAssembly.Equals(assemblySymbol, SymbolEqualityComparer.Default) && !typeSymbols.Contains(typeSymbol.BaseType))
            {
                queue.Enqueue(typeSymbol.BaseType);
            }
        }
    }

    private static SourceTypeInfo PaseEnum(INamedTypeSymbol typeSymbol)
    {
        SourceTypeInfo typeInfo = new()
        {
            Name = typeSymbol.Name,
            BaseType = typeSymbol.BaseType.ToDisplayString(GlobalTypeDisplayFormat),
            EnumUnderlyingType = typeSymbol.EnumUnderlyingType.ToDisplayString(GlobalTypeDisplayFormat),
            FullName = typeSymbol.ToDisplayString(GlobalTypeDisplayFormat),
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
            BaseType = typeSymbol.BaseType.ToDisplayString(GlobalTypeDisplayFormat),
            FullName = fullname,
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
                CanRead = property.GetMethod != null,
                CanWrite = property.SetMethod != null,
                IsInitOnly = property.SetMethod?.IsInitOnly == true,
                GetMethodAccessibility = property.GetMethod?.DeclaredAccessibility ?? Accessibility.NotApplicable,
                SetMethodAccessibility = property.SetMethod?.DeclaredAccessibility ?? Accessibility.NotApplicable,
                PropertyType = property.Type.ToDisplayString(GlobalTypeDisplayFormat),
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
                    NullableAnnotation = x.NullableAnnotation,
                    HasDefaultValue = x.HasExplicitDefaultValue,
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
                ReturnType = method.ReturnType.ToDisplayString(GlobalTypeDisplayFormat),
                ReturnNullableAnnotation = method.ReturnNullableAnnotation,
                Parameters = method.Parameters.Select(x => new SourceParameterInfo
                {
                    Name = x.Name,
                    ParameterType = x.Type.ToDisplayString(GlobalTypeDisplayFormat),
                    NullableAnnotation = x.NullableAnnotation,
                    HasDefaultValue = x.HasExplicitDefaultValue,
                    DefaultValue = x.HasExplicitDefaultValue ? x.ExplicitDefaultValue : null,
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

    private static string Emit(List<SourceTypeInfo> types, CancellationToken cancellationToken)
    {
        CSharpCodeBuilder builder = new();

        builder.AppendAutoGeneratedComment();
        builder.AppendBlock("internal static class __SourceReflectorInitializer", () =>
        {
            builder.AppendLine("[global::System.Runtime.CompilerServices.ModuleInitializer]");
            builder.AppendBlock("public static void Initialize()", () =>
            {
                foreach (SourceTypeInfo type in types)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    builder.AppendBlock("global::SourceGeneration.Reflection.SourceReflector.Add(new global::SourceGeneration.Reflection.SourceTypeInfo()", () =>
                    {
                        builder.AppendAssignment("Name", type.Name);
                        builder.AppendAssignment("IsStatic", type.IsStatic);
                        builder.AppendAssignment("IsRecord", type.IsRecord);
                        builder.AppendAssignment("IsStruct", type.IsStruct);
                        builder.AppendAssignment("IsEnum", type.IsEnum);
                        builder.AppendAssignment("IsReadOnly", type.IsReadOnly);

                        builder.AppendLine($"Type = typeof({type.FullName}),");
                        builder.AppendLine($"BaseType = typeof({type.BaseType}),");
                        if (type.EnumUnderlyingType != null)
                        {
                            builder.AppendLine($"EnumUnderlyingType = typeof({type.EnumUnderlyingType}),");
                        }
                        builder.AppendLine($"Accessibility = global::SourceGeneration.Reflection.SourceAccessibility.{type.Accessibility},");

                        if (type.Fields.Count > 0)
                        {
                            builder.AppendBlock("DeclaredFields = new global::SourceGeneration.Reflection.SourceFieldInfo[]", () =>
                            {
                                foreach (var field in type.Fields)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();

                                    builder.AppendIndent();
                                    builder.Append($"new global::SourceGeneration.Reflection.SourceFieldInfo(() => typeof({type.FullName}).GetField(\"{field.Name}\", ");
                                    AppendBindingFlags(builder, field.Accessibility, field.IsStatic);
                                    builder.Append("))");
                                    builder.AppendLine();

                                    builder.AppendBlock(() =>
                                    {
                                        builder.AppendLine($"FieldType = typeof({field.FieldType}),");
                                        builder.AppendLine($"Accessibility = global::SourceGeneration.Reflection.SourceAccessibility.{field.Accessibility},");
                                        builder.AppendLine($"NullableAnnotation = global::SourceGeneration.Reflection.SourceNullableAnnotation.{field.NullableAnnotation},");
                                        builder.AppendAssignment("Name", field.Name);
                                        builder.AppendAssignment("IsStatic", field.IsStatic);
                                        builder.AppendAssignment("IsRequired", field.IsRequired);
                                        builder.AppendAssignment("IsReadOnly", field.IsReadOnly);
                                        builder.AppendAssignment("IsConst", field.IsConst);

                                        if (!type.IsGenericTypeDefinition &&
                                            !type.IsRefLikeType &&
                                            field.Accessibility != Accessibility.Private && field.Accessibility != Accessibility.Protected)
                                        {
                                            if (field.IsConst)
                                            {
                                                builder.AppendLine($@"GetValue = _ => {CSharpCodeBuilder.GetConstantLiteral(field.ConstantValue)},");
                                            }
                                            else
                                            {
                                                builder.AppendLine($@"GetValue = instance => (({type.FullName})instance).{field.Name},");
                                            }

                                            if (!type.IsStruct && !field.IsReadOnly && !field.IsConst)
                                            {
                                                builder.AppendLine($@"SetValue = (instance, value) => (({type.FullName})instance).{field.Name} = ({field.FieldType})value");
                                            }
                                        }

                                    }, ",");
                                }
                            }, ",");
                        }
                        else
                        {
                            builder.AppendLine("DeclaredFields = global::System.Array.Empty<global::SourceGeneration.Reflection.SourceFieldInfo>(),");
                        }

                        if (type.Properties.Count > 0)
                        {
                            builder.AppendBlock("DeclaredProperties = new global::SourceGeneration.Reflection.SourcePropertyInfo[]", () =>
                            {
                                foreach (var property in type.Properties)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();

                                    builder.AppendIndent();
                                    builder.Append($"new global::SourceGeneration.Reflection.SourcePropertyInfo(() => typeof({type.FullName}).GetProperty(\"{property.Name}\", ");
                                    AppendBindingFlags(builder, property.Accessibility, property.IsStatic);
                                    builder.Append("))");
                                    builder.AppendLine();

                                    builder.AppendBlock(() =>
                                    {
                                        builder.AppendLine($"PropertyType = typeof({property.PropertyType}),");
                                        builder.AppendLine($"Accessibility = global::SourceGeneration.Reflection.SourceAccessibility.{property.Accessibility},");
                                        builder.AppendLine($"NullableAnnotation = global::SourceGeneration.Reflection.SourceNullableAnnotation.{property.NullableAnnotation},");
                                        builder.AppendAssignment("Name", property.Name);
                                        builder.AppendAssignment("IsVirtual", property.IsVirtual);
                                        builder.AppendAssignment("IsStatic", property.IsStatic);
                                        builder.AppendAssignment("IsRequired", property.IsRequired);
                                        builder.AppendAssignment("IsAbstract", property.IsAbstract);
                                        builder.AppendAssignment("IsInitOnly", property.IsInitOnly);

                                        builder.AppendAssignment("CanWrite", property.CanWrite);
                                        builder.AppendAssignment("CanRead", property.CanRead);

                                        if (!type.IsGenericTypeDefinition &&
                                            !type.IsRefLikeType &&
                                            property.Accessibility != Accessibility.Private && property.Accessibility != Accessibility.Protected)
                                        {
                                            if (property.CanRead && property.GetMethodAccessibility != Accessibility.Private && property.GetMethodAccessibility != Accessibility.Protected)
                                            {
                                                builder.AppendLine($@"GetValue = instance => (({type.FullName})instance).{property.Name},");
                                            }

                                            if (!type.IsStruct && !property.IsInitOnly && property.CanWrite && property.SetMethodAccessibility != Accessibility.Private && property.SetMethodAccessibility != Accessibility.Protected)
                                            {
                                                builder.AppendLine($@"SetValue = (instance, value) => (({type.FullName})instance).{property.Name} = ({property.PropertyType})value");
                                            }
                                        }

                                    }, ",");
                                }
                            }, ",");
                        }
                        else
                        {
                            builder.AppendLine("DeclaredProperties = global::System.Array.Empty<global::SourceGeneration.Reflection.SourcePropertyInfo>(),");
                        }

                        if (type.Methods.Count > 0)
                        {
                            builder.AppendBlock("DeclaredMethods = new global::SourceGeneration.Reflection.SourceMethodInfo[]", () =>
                            {
                                foreach (var method in type.Methods)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();

                                    builder.AppendIndent();
                                    builder.Append($"new global::SourceGeneration.Reflection.SourceMethodInfo(() => typeof({type.FullName}).GetMethod(\"{method.Name}\", ");
                                    AppendBindingFlags(builder, method.Accessibility, method.IsStatic);
                                    builder.Append(", ");
                                    AppendParametersTypeArray(builder, method.Parameters);
                                    builder.Append("))");
                                    builder.AppendLine();

                                    builder.AppendBlock(() =>
                                    {
                                        builder.AppendLine($"Accessibility = global::SourceGeneration.Reflection.SourceAccessibility.{method.Accessibility},");
                                        builder.AppendLine($"ReturnType = typeof({method.ReturnType}),");
                                        builder.AppendLine($"ReturnNullableAnnotation = global::SourceGeneration.Reflection.SourceNullableAnnotation.{method.ReturnNullableAnnotation},");

                                        builder.AppendAssignment("Name", method.Name);
                                        builder.AppendAssignment("IsStatic", method.IsStatic);

                                        AppendParametersProperty(builder, method.Parameters);

                                        if (!type.IsGenericTypeDefinition &&
                                            !type.IsRefLikeType &&
                                            method.Accessibility != Accessibility.Private && method.Accessibility != Accessibility.Protected)
                                        {
                                            builder.AppendIndent();
                                            builder.Append("Invoke = (instance, parameters) => ");

                                            if (method.ReturnType == "void")
                                            {
                                                builder.Append("{ ");
                                            }

                                            if (method.IsStatic)
                                            {
                                                builder.Append($"{type.FullName}.{method.Name}(");
                                            }
                                            else
                                            {
                                                builder.Append($"(({type.FullName})instance).{method.Name}(");
                                            }
                                            for (int i = 0; i < method.Parameters.Count; i++)
                                            {
                                                SourceParameterInfo parameter = method.Parameters[i];
                                                builder.Append($"({parameter.ParameterType})parameters[{i}]");
                                                if (i < method.Parameters.Count - 1)
                                                {
                                                    builder.Append(", ");
                                                }
                                            }
                                            builder.Append(")");

                                            if (method.ReturnType == "void")
                                            {
                                                builder.Append("; return null; }");
                                            }

                                            builder.AppendLine();
                                        }

                                    }, ",");
                                }
                            }, ",");
                        }
                        else
                        {
                            builder.AppendLine("DeclaredMethods = global::System.Array.Empty<global::SourceGeneration.Reflection.SourceMethodInfo>(),");
                        }

                        if (type.Constructors.Count > 0)
                        {
                            builder.AppendBlock("DeclaredConstructors = new global::SourceGeneration.Reflection.SourceConstructorInfo[]", () =>
                            {
                                foreach (var constructor in type.Constructors)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();

                                    builder.AppendIndent();
                                    builder.Append($"new global::SourceGeneration.Reflection.SourceConstructorInfo(() => typeof({type.FullName}).GetConstructor(");
                                    AppendBindingFlags(builder, constructor.Accessibility, constructor.IsStatic);
                                    builder.Append(", ");
                                    AppendParametersTypeArray(builder, constructor.Parameters);
                                    builder.Append("))");
                                    builder.AppendLine();

                                    builder.AppendBlock(() =>
                                    {
                                        builder.AppendLine($"Accessibility = global::SourceGeneration.Reflection.SourceAccessibility.{constructor.Accessibility},");
                                        builder.AppendAssignment("Name", constructor.Name);
                                        builder.AppendAssignment("IsStatic", constructor.IsStatic);

                                        if (constructor.IsStatic)
                                        {
                                            builder.AppendLine("Parameters = global::System.Array.Empty<global::SourceGeneration.Reflection.SourceParameterInfo>(),");
                                        }
                                        else
                                        {
                                            AppendParametersProperty(builder, constructor.Parameters);

                                            if (!type.IsGenericTypeDefinition &&
                                                !type.IsRefLikeType &&
                                                constructor.Accessibility != Accessibility.Private && constructor.Accessibility != Accessibility.Protected &&
                                                !type.Properties.Any(x => x.IsRequired))
                                            {
                                                builder.AppendIndent();
                                                builder.Append("Invoke = (parameters) => ");
                                                builder.Append($"new {type.FullName}(");
                                                for (int i = 0; i < constructor.Parameters.Count; i++)
                                                {
                                                    SourceParameterInfo parameter = constructor.Parameters[i];
                                                    builder.Append($"({parameter.ParameterType})parameters[{i}]");
                                                    if (i < constructor.Parameters.Count - 1)
                                                    {
                                                        builder.Append(", ");
                                                    }
                                                }
                                                builder.Append(")");
                                                builder.AppendLine();

                                                //if (type.Properties.Any(x => x.IsRequired))
                                                //{
                                                //    builder.AppendBlock(() =>
                                                //    {
                                                //        foreach (var property in type.Properties.Where(x => x.IsRequired))
                                                //        {
                                                //            builder.AppendLine($"{property.Name} = default,");
                                                //        }
                                                //    });
                                                //}
                                            }
                                        }

                                    }, ",");
                                }
                            });
                        }
                        else
                        {
                            builder.AppendLine("DeclaredConstructors = global::System.Array.Empty<global::SourceGeneration.Reflection.SourceConstructorInfo>(),");
                        }

                    }, ");");
                }
            });
        });

        return builder.ToString();
    }

    private static void AppendBindingFlags(CSharpCodeBuilder builder, Accessibility accessibility, bool isStatic)
    {
        if (isStatic)
        {
            builder.Append("global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Static");
        }
        else
        {
            builder.Append("global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Instance");
        }

        if (accessibility == Accessibility.Public)
        {
            builder.Append(" | global::System.Reflection.BindingFlags.Public");
        }
        else
        {
            builder.Append(" | global::System.Reflection.BindingFlags.NonPublic");
        }
    }

    private static void AppendParametersTypeArray(CSharpCodeBuilder builder, List<SourceParameterInfo> parameters)
    {
        if (parameters.Count == 0)
        {
            builder.Append("global::System.Array.Empty<global::System.Type>()");
        }
        else
        {
            builder.Append("new global::System.Type[] { ");
            for (int i = 0; i < parameters.Count; i++)
            {
                SourceParameterInfo parameter = parameters[i];
                builder.Append($"typeof({parameter.ParameterType})");
                if (i < parameters.Count - 1)
                {
                    builder.Append(", ");
                }
            }
            builder.Append(" }");
        }
    }

    private static void AppendParametersProperty(CSharpCodeBuilder builder, List<SourceParameterInfo> parameters)
    {
        if (parameters.Count == 0)
        {
            builder.AppendLine("Parameters = Array.Empty<global::SourceGeneration.Reflection.SourceParameterInfo>(),");
        }
        else
        {
            builder.AppendBlock("Parameters = new global::SourceGeneration.Reflection.SourceParameterInfo[]", () =>
            {
                foreach (var parameter in parameters)
                {
                    builder.AppendBlock("new global::SourceGeneration.Reflection.SourceParameterInfo()", () =>
                    {
                        builder.AppendAssignment("Name", parameter.Name);
                        builder.AppendAssignment("HasDefaultValue", parameter.HasDefaultValue);
                        builder.AppendAssignment("DefaultValue", parameter.DefaultValue);
                        builder.AppendLine($"ParameterType = typeof({parameter.ParameterType}),");
                        builder.AppendLine($"NullableAnnotation = global::SourceGeneration.Reflection.SourceNullableAnnotation.{parameter.NullableAnnotation},");
                    }, ",");
                }
            }, ",");
        }
    }


    //private static void OutputGlobalConfigKeys(SourceProductionContext context, AnalyzerConfigOptions options)
    //{
    //    CSharpCodeBuilder builder = new();
    //    builder.AppendBlock("public class GlobalConfig", () =>
    //    {
    //        int i = 0;
    //        foreach (var key in options.Keys)
    //        {
    //            if (key.EndsWith("attribute"))
    //            {
    //                builder.AppendLine($"public readonly string Key_{i} = \"{key}\";");

    //                options.TryGetValue(key, out var value);
    //                builder.AppendLine($"public readonly string Value_{i} = \"{value}\";");

    //                builder.AppendLine();
    //                i++;
    //            }
    //        }
    //    });
    //    context.AddSource("GlobalConfig.g.cs", builder.ToString());
    //}
    //private static void OutputAdditionalAttributes(SourceProductionContext context)
    //{
    //    CSharpCodeBuilder builder = new();
    //    builder.AppendBlock("public class AdditionalAttributes", () =>
    //    {
    //        int i = 0;
    //        foreach (var key in SourceReflectionTypes)
    //        {
    //            builder.AppendLine($"public readonly string Key_{i} = \"{key}\";");
    //            builder.AppendLine();
    //            i++;
    //        }
    //    });
    //    context.AddSource("AdditionalAttributes.g.cs", builder.ToString());
    //}
}
