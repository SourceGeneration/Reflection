using CustomLibrary1;
using CustomLibrary2;
using SourceGeneration.Reflection;
using System.ComponentModel.DataAnnotations;

var types = SourceReflector.GetTypes();

foreach (var type in types)
{
    Console.WriteLine("Type: " + type.Name);
}

Console.ReadLine();

[SourceReflection]
public class A { }

[YourReflection]
public class B { }

[YourOtherReflection]
public class C { }

[Display]
public class E { }

public class D { }
