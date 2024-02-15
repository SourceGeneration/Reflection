#if !NET8_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, Inherited = false)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    internal sealed class NullableAttribute : Attribute
    {
        public readonly byte[] NullableFlags;
        public NullableAttribute(byte value) => NullableFlags = [value];
        public NullableAttribute(byte[] value) => NullableFlags = value;
    }
}
#endif

