using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace SourceGeneration.Reflection;

[Generator(LanguageNames.CSharp)]
public partial class ReflectionSourceGenerator : IIncrementalGenerator
{
    private const string SourceReflectionAttributeName = "SourceGeneration.Reflection.SourceReflectionAttribute";
    private const string SourceReflectionTypeAttributeName = "SourceGeneration.Reflection.SourceReflectionTypeAttribute";
    private static string[] CustomSourceReflectionAttributes = [];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
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
                if (node is AttributeSyntax attribute)
                {
                    string name;
                    if (attribute.Name is GenericNameSyntax genericName &&
                        genericName.TypeArgumentList.Arguments.Count == 1)
                    {
                        name = genericName.Identifier.Text.Trim();
                    }
                    else if (attribute.Name is IdentifierNameSyntax identifierName)
                    {
                        name = identifierName.Identifier.Text.Trim();
                    }
                    else
                    {
                        return false;
                    }

                    if (!name.EndsWith("Attribute"))
                        name += "Attribute";

                    if (MatchFullName(SourceReflectionAttributeName, name))
                        return true;

                    if (MatchFullName(SourceReflectionTypeAttributeName, name))
                        return true;

                    if (CustomSourceReflectionAttributes.Any(x => MatchFullName(x, name)))
                        return true;
                }

                return false;
            },
            static (context, cancellationToken) =>
            {
                return (AttributeSyntax)context.Node;
            });

        context.RegisterSourceOutput(results.Collect().Combine(context.CompilationProvider), static (context, source) =>
        {
            CancellationToken cancellationToken = context.CancellationToken;
            var compilation = source.Right;
            var assembly = compilation.Assembly;
            var attributes = source.Left;

            List<INamedTypeSymbol> namedTypeSymbols = [];
            foreach (var group in attributes.GroupBy(x => x.SyntaxTree))
            {
                var semanticModel = compilation.GetSemanticModel(group.Key);

                foreach (var attribute in group)
                {
                    if (attribute.Name is GenericNameSyntax genericName)
                    {
                        var type = compilation.GetSemanticModel(attribute.SyntaxTree).GetTypeInfo(genericName.TypeArgumentList.Arguments[0], cancellationToken);
                        if (type.ConvertedType is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeKind != TypeKind.Error)
                        {
                            namedTypeSymbols.Add(namedTypeSymbol);
                        }
                    }
                    else if (attribute.ArgumentList != null && attribute.ArgumentList.Arguments.Count == 1)
                    {
                        if(attribute.ArgumentList.Arguments[0].Expression is TypeOfExpressionSyntax typeOfExpression)
                        {
                            var type = compilation.GetSemanticModel(attribute.SyntaxTree).GetTypeInfo(typeOfExpression.Type, cancellationToken);
                            if (type.ConvertedType is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeKind != TypeKind.Error)
                            {
                                namedTypeSymbols.Add(namedTypeSymbol);
                            }
                        }
                    }
                    else
                    {
                        var type = (INamedTypeSymbol)compilation.GetSemanticModel(attribute.SyntaxTree).GetDeclaredSymbol(attribute.Parent.Parent, cancellationToken);
                        if (type is INamedTypeSymbol namedTypeSymbol &&
                            (namedTypeSymbol.TypeKind == TypeKind.Class ||
                            namedTypeSymbol.TypeKind == TypeKind.Struct ||
                            namedTypeSymbol.TypeKind == TypeKind.Enum))
                        {
                            namedTypeSymbols.Add(namedTypeSymbol);
                        }
                    }
                }
            }

            if (namedTypeSymbols.Count == 0)
                return;


            var types = Parse(assembly, namedTypeSymbols, cancellationToken).ToList();
            if (namedTypeSymbols.Count == 0)
                return;

            Emit(context, types, cancellationToken);

            context.AddSource("__SourceReflection.ModuleInitializerAttribute.g.cs", @"#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    internal sealed class ModuleInitializerAttribute : System.Attribute { }
}
#endif");

            //context.AddSource("__SourceReflection.SourceReflectorInitializer.g.cs", code);
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

}
