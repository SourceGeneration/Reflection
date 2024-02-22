using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace SourceGeneration.Reflection;

internal class SourceMethodBase
{
    public string Name;
    public bool IsStatic;
    public List<SourceParameterInfo> Parameters = [];
    public Accessibility Accessibility;
}
