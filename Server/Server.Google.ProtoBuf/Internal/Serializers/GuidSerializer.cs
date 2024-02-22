using System;
using System.Diagnostics;

namespace ProtoBuf.Internal.Serializers
{
    internal sealed class GuidSerializer : IRuntimeProtoSerializerNode
    {
        bool IRuntimeProtoSerializerNode.IsScalar
        {
            get
            {
                switch (_variant)
                {
                    case Variant.GuidString:
                    case Variant.GuidBytes:
                        return true;
                    default:
                        return false;
                }
            }
        }

        private enum Variant
        {
            BclGuid = 0,
            GuidString = 1,
            GuidBytes = 2,
        }
        private readonly Variant _variant;
        private static GuidSerializer s_Legacy, s_String, s_Bytes;

        internal static GuidSerializer Create(CompatibilityLevel compatibilityLevel, DataFormat dataFormat)
        {
            if (compatibilityLevel < CompatibilityLevel.Level300)
                return s_Legacy = s_Legacy ?? new GuidSerializer(Variant.BclGuid);
            if (dataFormat == DataFormat.FixedSize)
                return s_Bytes = s_Bytes ?? new GuidSerializer(Variant.GuidBytes);
            return s_String = s_String ?? new GuidSerializer(Variant.GuidString);
        }

        private GuidSerializer(Variant variant) => _variant = variant;

        private static readonly Type expectedType = typeof(Guid);

        public Type ExpectedType { get { return expectedType; } }

        bool IRuntimeProtoSerializerNode.RequiresOldValue => false;

        bool IRuntimeProtoSerializerNode.ReturnsValue => true;

        public void Write(ref ProtoWriter.State state, object value)
        {
            switch (_variant)
            {
                case Variant.GuidString:
                    BclHelpers.WriteGuidString(ref state, (Guid)value);
                    break;
                case Variant.GuidBytes:
                    BclHelpers.WriteGuidBytes(ref state, (Guid)value);
                    break;
                default:
                    BclHelpers.WriteGuid(ref state, (Guid)value);
                    break;
            }
        }

        public object Read(ref ProtoReader.State state, object value)
        {
            Debug.Assert(value is null); // since replaces
            switch (_variant)
            {
                case Variant.GuidString:
                    return BclHelpers.ReadGuidString(ref state);
                case Variant.GuidBytes:
                    return BclHelpers.ReadGuidBytes(ref state);
                default:
                    return BclHelpers.ReadGuid(ref state);
            }
        }

        void IRuntimeProtoSerializerNode.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
        {
            ctx.EmitStateBasedWrite(
                _variant == Variant.GuidString ? nameof(BclHelpers.WriteGuidString) : _variant == Variant.GuidBytes ? nameof(BclHelpers.WriteGuidBytes) : nameof(BclHelpers.WriteGuid), valueFrom, typeof(BclHelpers));
        }

        void IRuntimeProtoSerializerNode.EmitRead(Compiler.CompilerContext ctx, Compiler.Local entity)
        {
            ctx.EmitStateBasedRead(typeof(BclHelpers),
                _variant == Variant.GuidString ? nameof(BclHelpers.ReadGuidString) : _variant == Variant.GuidBytes ? nameof(BclHelpers.ReadGuidBytes) : nameof(BclHelpers.ReadGuid), ExpectedType);
        }
    }
}