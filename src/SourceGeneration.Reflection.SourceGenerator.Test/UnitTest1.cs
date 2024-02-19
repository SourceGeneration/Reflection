namespace SourceGeneration.Reflection.SourceGenerator.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            string source = @"
[assembly: SourceReflectionAttribute<SourceReflectionAttribute>]
using System;
using SourceGeneration.Reflection;

namespace SourceGeneration.Reflection.Sample2
{
    public class D 
    {  
           [SourceReflection]
           protected class NestedClass { }
    }
}
";
            var result = CSharpTestGenerator.Generate<ReflectionSourceGenerator>(source, typeof(SourceReflectionAttribute).Assembly);
            var script = result.RunResult.GeneratedTrees.FirstOrDefault()?.GetText();
            var script2 = result.RunResult.GeneratedTrees.LastOrDefault()?.GetText();
            
        }
    }
}

