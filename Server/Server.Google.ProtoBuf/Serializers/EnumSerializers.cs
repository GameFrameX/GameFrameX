using ProtoBuf.Meta;
using System;
using System.Reflection;

namespace ProtoBuf.Serializers
{
    internal static class EnumSerializers
    {
        internal static object GetSerializer(Type type)
        {
            switch (RuntimeTypeModel.GetUnderlyingProvider(GetProvider(type), type))
            {
                case FieldInfo field:
                    return field.GetValue(null);
                case MethodInfo method:
                    return method.Invoke(null, null);
                default:
                    return null;
            }
        }

        internal static MemberInfo GetProvider(Type type)
        {
            if (type is null) return null;
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (!type.IsEnum) return null;
            string name;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                    name = nameof(EnumSerializer.CreateSByte);
                    break;
                case TypeCode.Int16:
                    name = nameof(EnumSerializer.CreateInt16);
                    break;
                case TypeCode.Int32:
                    name = nameof(EnumSerializer.CreateInt32);
                    break;
                case TypeCode.Int64:
                    name = nameof(EnumSerializer.CreateInt64);
                    break;
                case TypeCode.Byte:
                    name = nameof(EnumSerializer.CreateByte);
                    break;
                case TypeCode.UInt16:
                    name = nameof(EnumSerializer.CreateUInt16);
                    break;
                case TypeCode.UInt32:
                    name = nameof(EnumSerializer.CreateUInt32);
                    break;
                case TypeCode.UInt64:
                    name = nameof(EnumSerializer.CreateUInt64);
                    break;
                default:
                    name = null;
                    break;
            }

            if (name is null) return null;
            return typeof(EnumSerializer).GetMethod(name, BindingFlags.Static | BindingFlags.Public)
                .MakeGenericMethod(type);
        }
    }
}