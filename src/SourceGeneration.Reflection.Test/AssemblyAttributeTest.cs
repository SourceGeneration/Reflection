using SourceGeneration.Reflection;

[assembly: SourceReflectionType<object>]
[assembly: SourceReflectionType<int>]
[assembly: SourceReflectionType<string>]
[assembly: SourceReflectionType(typeof(Enumerable))]
[assembly: SourceReflectionType(typeof(List<>))]
[assembly: SourceReflectionType<Array>]
[assembly: SourceReflectionType<Task>]
[assembly: SourceReflectionType<FileStream>]
[assembly: SourceReflectionType<ValueTask>]