using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Reflection;

namespace SourceGeneration.Reflection.SourceGenerator.Test;

public static class CSharpTestGenerator
{
    public static GenerationResult Generate<T>(string source, params Assembly[] addReferences) where T : IIncrementalGenerator, new()
    {
        List<Assembly> assemblies = [typeof(T).Assembly];

        if (addReferences != null)
        {
            assemblies.AddRange(addReferences);
        }

        var generator = new T();
        var result = CSharpGeneratorDriver
            .Create(generator)
            .RunGeneratorsAndUpdateCompilation(
                CreateLibrary(source, [.. assemblies]),
                out var outputCompilation,
                out var diagnostics)
            .GetRunResult();

        return new(result, outputCompilation, diagnostics);
    }

    private static readonly Assembly[] ImportantAssemblies =
    [
        typeof(object).Assembly,
        typeof(MulticastDelegate).Assembly
    ];

    private static Assembly[] AssemblyReferencesForCodegen =>
        AppDomain.CurrentDomain
            .GetAssemblies()
            .Concat(ImportantAssemblies)
            .Distinct()
            .Where(a => !a.IsDynamic)
            .ToArray();

    private static CSharpCompilation CreateLibrary(string source, params Assembly[] addReferences)
    {
        var references = new List<MetadataReference>();
        var assemblies = AssemblyReferencesForCodegen;
        foreach (Assembly assembly in assemblies)
        {
            if (!assembly.IsDynamic)
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }
        if (addReferences != null)
        {
            foreach (var assembly in addReferences)
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        var compilation = CSharpCompilation.Create(
            "compilation",
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        return compilation;
    }

}

public class GenerationResult(
    GeneratorDriverRunResult runResult,
    Compilation compilation,
    ImmutableArray<Diagnostic> diagnostics)
{
    public GeneratorDriverRunResult RunResult { get; } = runResult;
    public Compilation Compilation { get; } = compilation;
    public ImmutableArray<Diagnostic> Diagnostics { get; } = diagnostics;
}
