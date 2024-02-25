namespace SourceGeneration.Reflection.SourceGenerator.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            string source = @"
//[assembly: SourceReflectionAttribute<SourceGeneration.Reflection.Sample2.EnumTestObject>]
[assembly: SourceReflectionType(typeof(System.Collections.Generic.List<string>))]
using System;
using System.Threading.Tasks;
using SourceGeneration.Reflection;

namespace SourceGeneration.Reflection.Sample2
{
    //[SourceReflection]
    //public class IndexTestObject
    //{
    //    public string this[int a]
    //    {
    //        get => string.Empty;
    //        set { }
    //    }
    //}
}
";
            var result = CSharpTestGenerator.Generate<ReflectionSourceGenerator>(source, typeof(SourceReflectionAttribute).Assembly);
            var script = result.RunResult.GeneratedTrees.FirstOrDefault()?.GetText();
            var script2 = result.RunResult.GeneratedTrees.LastOrDefault()?.GetText();
        }
    }
}