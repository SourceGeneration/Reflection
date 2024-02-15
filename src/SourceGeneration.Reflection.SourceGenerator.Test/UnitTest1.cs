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
    [SourceReflection]
    public class D 
    {  

const string A = ""abc"";
const string B = A;
            public string name;
    private string a;
    }
}
";
            var result = CSharpTestGenerator.Generate<ReflectionSourceGenerator>(source, typeof(SourceReflectionAttribute).Assembly);
            var script = result.RunResult.GeneratedTrees.FirstOrDefault()?.GetText();
            var script2 = result.RunResult.GeneratedTrees.LastOrDefault()?.GetText();
        }
    }
}