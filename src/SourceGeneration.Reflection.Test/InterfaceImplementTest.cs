using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceGeneration.Reflection.Test;

public class InterfaceImplementTest
{
}

[SourceReflection]
public class InterfaceImplementTestObject : System.ICloneable, System.IComparable
{
    public int CompareTo(object? obj)
    {
        throw new NotImplementedException();
    }

    object System.ICloneable.Clone()
    {
        throw new NotImplementedException();
    }
}
