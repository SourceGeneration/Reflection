namespace SourceGeneration.Reflection.SourceGenerator.Test;

public class List2<T> : List<T> { }

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
        string source = """
            //[assembly: SourceReflectionAttribute<SourceGeneration.Reflection.Sample2.EnumTestObject>]
            //[assembly: SourceReflectionType(typeof(System.Collections.Generic.List<string>))]
            //[assembly: SourceReflectionType<int[]>]
            using System;
            using System.Threading.Tasks;
            using SourceGeneration.Reflection;

            namespace SourceGeneration.Reflection.Sample2
            {
                [SourceReflection]
                public class BaseClass 
                { 
                    public int A { get; set; }  = 1;
                    public int B  = 2;
                }
            }
            """;

        var result = CSharpTestGenerator.Generate<ReflectionSourceGenerator>(source, typeof(SourceReflectionAttribute).Assembly, typeof(UnitTest1).Assembly);
        var script = result.RunResult.GeneratedTrees.FirstOrDefault()?.GetText();
        var script2 = result.RunResult.GeneratedTrees.LastOrDefault()?.GetText();
    }
}