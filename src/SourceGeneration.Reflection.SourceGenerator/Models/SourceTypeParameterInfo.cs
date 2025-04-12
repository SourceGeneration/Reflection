namespace SourceGeneration.Reflection;

internal class SourceTypeParameterInfo
{
    public string Name;

    public bool HasUnmanagedTypeConstraint;
    public bool HasValueTypeConstraint;

    public bool HasTypeParameterInConstraintTypes;
    public string[] ConstraintTypes;
}