using SourceGeneration.Reflection;

[assembly: SourceReflectionType<object>]
[assembly: SourceReflectionType<Array>]
[assembly: SourceReflectionType(typeof(Enumerable))]
[assembly: SourceReflectionType(typeof(List<>))]

namespace SourceGeneration.Reflection.Test;