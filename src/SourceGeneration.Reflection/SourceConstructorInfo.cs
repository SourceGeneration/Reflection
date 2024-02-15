using System.Reflection;

namespace SourceGeneration.Reflection;

public class SourceConstructorInfo(Func<ConstructorInfo> getConstructorInfo) : SourceMethodBase
{
    private ConstructorInfo? _constructorInfo;
    private Func<object?[]?, object>? _invoke;

    public ConstructorInfo ConstructorInfo => _constructorInfo ??= getConstructorInfo();

    public Func<object?[]?, object> Invoke
    {
        get => _invoke ?? ConstructorInfo.Invoke;
        init => _invoke = value;
    }
}
