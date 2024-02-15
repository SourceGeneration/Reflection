using CustomLibrary1;
using CustomLibrary2;
using SourceGeneration.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ConsoleApp;

[SourceReflection]
public class A { }

[YourReflection]
public class B { }

[YourOtherReflection]
public class C { }

[Display]
public class E { }

public class D { }
