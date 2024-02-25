namespace SourceGeneration.Reflection;

internal class SourceTypeParameterInfo
{
    public string Name;

    public bool HasUnmanagedTypeConstraint;

    public string[] ConstraintTypes;
}