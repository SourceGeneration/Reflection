using System.Diagnostics.CodeAnalysis;

namespace SourceGeneration.AutoMapper;

public static class TypeExtensions
{
    private static readonly Type NullableType = typeof(Nullable<>);
    private static readonly Type EnumerableGenericInterfaceType = typeof(IEnumerable<>);
    private static readonly Type DictionaryGenericInterfaceType = typeof(IDictionary<,>);

    public static bool IsNullableType(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == NullableType;

    public static bool IsIntegerType(this Type type) =>
        type == typeof(byte)
        || type == typeof(sbyte)
        || type == typeof(short)
        || type == typeof(ushort)
        || type == typeof(int)
        || type == typeof(uint)
        || type == typeof(long)
        || type == typeof(ulong)
#if NET7_0_OR_GREATER
        || type == typeof(Int128)
        || type == typeof(UInt128)
#endif
        ;

    public static bool IsFloatingPointType(this Type type) =>
        type == typeof(float)
        || type == typeof(double)
        || type == typeof(decimal)
#if NET5_0_OR_GREATER
        || type == typeof(Half)
#endif
        ;

    public static bool IsNumberType(this Type type) => type.IsIntegerType() || type.IsFloatingPointType();

    public static bool IsCompatibleEnumerableGenericInterface(this Type type, out Type elementType)
    {
        var interfaceType = GetCompatibleGenericInterface(type, EnumerableGenericInterfaceType);
        if (interfaceType == null)
        {
            elementType = null!;
            return false;
        }

        elementType = interfaceType.GenericTypeArguments[0];
        return true;
    }

    public static bool IsCompatibleDictionaryGenericInterface(this Type type, out Type keyType, out Type valueType)
    {
        var interfaceType = GetCompatibleGenericInterface(type, DictionaryGenericInterfaceType);
        if (interfaceType == null)
        {
            keyType = null!;
            valueType = null!;
            return false;
        }

        keyType = interfaceType.GenericTypeArguments[0];
        valueType = interfaceType.GenericTypeArguments[1];
        return true;
    }

    public static Type? GetCompatibleGenericInterface(this Type type, Type interfaceType)
    {
        //Debug.Assert(interfaceType.IsGenericType);
        //Debug.Assert(interfaceType.IsInterface);
        //Debug.Assert(interfaceType == interfaceType.GetGenericTypeDefinition());

        Type interfaceToCheck = type;

        if (interfaceToCheck.IsGenericType)
        {
            interfaceToCheck = interfaceToCheck.GetGenericTypeDefinition();
        }

        if (interfaceToCheck == interfaceType)
        {
            return type;
        }

        foreach (Type typeToCheck in type.GetInterfaces())
        {
            if (typeToCheck.IsGenericType)
            {
                Type genericInterfaceToCheck = typeToCheck.GetGenericTypeDefinition();
                if (genericInterfaceToCheck == interfaceType)
                {
                    return typeToCheck;
                }
            }
        }

        return null;
    }

}
