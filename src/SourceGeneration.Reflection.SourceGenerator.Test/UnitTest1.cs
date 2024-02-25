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
[assembly: SourceReflectionType(typeof(object))]
using System;
using System.Threading.Tasks;
using SourceGeneration.Reflection;

namespace SourceGeneration.Reflection.Sample2
{

[SourceReflection]
public class A
{
    //public T Get<T>(T a, T[] value,Action<T,Func<T,Task<T>>> action) => value[0];
    //public T Get<T>(T a) => a;
}
    public enum EnumTestObject
    {
        A,B,
    }
}
";
            var result = CSharpTestGenerator.Generate<ReflectionSourceGenerator>(source, typeof(SourceReflectionAttribute).Assembly);
            var script = result.RunResult.GeneratedTrees.FirstOrDefault()?.GetText();
            var script2 = result.RunResult.GeneratedTrees.LastOrDefault()?.GetText();
        }
    }
}