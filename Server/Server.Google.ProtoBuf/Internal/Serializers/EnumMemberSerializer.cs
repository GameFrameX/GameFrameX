using ProtoBuf.Compiler;
using System;

namespace ProtoBuf.Internal.Serializers
{
    internal class EnumMemberSerializer : IRuntimeProtoSerializerNode, IDirectWriteNode
    {
        bool IRuntimeProtoSerializerNode.IsScalar => true;

        private readonly IRuntimeProtoSerializerNode _tail;
        public EnumMemberSerializer(Type enumType)
        {
            ExpectedType = enumType ?? throw new ArgumentNullException(nameof(enumType));
            if (!enumType.IsEnum) ThrowHelper.ThrowInvalidOperationException("Expected an enum type; got " + enumType.NormalizeName());

            switch (Type.GetTypeCode(enumType))
            {
                case TypeCode.SByte:
                    _tail = SByteSerializer.Instance;
                    break;
                case TypeCode.Int16:
                    _tail = Int16Serializer.Instance;
                    break;
                case TypeCode.Int32:
                    _tail = Int32Serializer.Instance;
                    break;
                case TypeCode.Int64:
                    _tail = Int64Serializer.Instance;
                    break;
                case TypeCode.Byte:
                    _tail = ByteSerializer.Instance;
                    break;
                case TypeCode.UInt16:
                    _tail = UInt16Serializer.Instance;
                    break;
                case TypeCode.UInt32:
                    _tail = UInt32Serializer.Instance;
                    break;
                case TypeCode.UInt64:
                    _tail = UInt64Serializer.Instance;
                    break;
                default:
                    _tail = default;
                    break;
            }

            if (_tail is null) ThrowHelper.ThrowInvalidOperationException("Unable to resolve underlying enum type for " + enumType.NormalizeName());

        }

        public Type ExpectedType { get; }

        bool IRuntimeProtoSerializerNode.RequiresOldValue => false;

        bool IRuntimeProtoSerializerNode.ReturnsValue => true;

        internal static object EnumToWire(object value, Type type)
        {
            unchecked
            {
                switch (Type.GetTypeCode(type))
                {
                    // unbox as the intended type
                    case TypeCode.Byte:
                        return (byte)value;
                    case TypeCode.SByte:
                        return (sbyte)value;
                    case TypeCode.Int16:
                        return (short)value;
                    case TypeCode.Int32:
                        return (int)value;
                    case TypeCode.Int64:
                        return (long)value;
                    case TypeCode.UInt16:
                        return (ushort)(ushort)value;
                    case TypeCode.UInt32:
                        return (uint)(uint)value;
                    case TypeCode.UInt64:
                        return (ulong)(ulong)value;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }
        private object EnumToWire(object value) => EnumToWire(value, ExpectedType);

        public object Read(ref ProtoReader.State state, object value)
            => Enum.ToObject(ExpectedType, _tail.Read(ref state, value));

        public void Write(ref ProtoWriter.State state, object value)
            => _tail.Write(ref state, EnumToWire(value));

        void IRuntimeProtoSerializerNode.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
            => _tail.EmitWrite(ctx, valueFrom);

        void IRuntimeProtoSerializerNode.EmitRead(Compiler.CompilerContext ctx, Compiler.Local entity)
            => _tail.EmitRead(ctx, entity);

        bool IDirectWriteNode.CanEmitDirectWrite(WireType wireType) => _tail is IDirectWriteNode dw && dw.CanEmitDirectWrite(wireType);

        void IDirectWriteNode.EmitDirectWrite(int fieldNumber, WireType wireType, CompilerContext ctx, Local valueFrom)
            => ((IDirectWriteNode)_tail).EmitDirectWrite(fieldNumber, wireType, ctx, valueFrom);
    }
}