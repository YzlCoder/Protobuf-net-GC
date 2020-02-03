using System;
using ProtoBuf.Meta;

namespace ProtoBuf.Extension
{
    sealed class Color32Serializer : Serializers.IProtoTypeSerializer
    {
        public Type ExpectedType { get { return typeof(UnityEngine.Color32); } }

        public bool RequiresOldValue { get { return true; } }

        public bool ReturnsValue { get { return true; } }

        public void Callback(object value, TypeModel.CallbackType callbackType, SerializationContext context)
        {
            
        }

        public bool CanCreateInstance()
        {
            return true;
        }

        public object CreateInstance(ProtoReader source)
        {
            return new UnityEngine.Color32();
        }

        public bool HasCallbacks(TypeModel.CallbackType callbackType)
        {
            return false;
        }

        public object Read(object value, ProtoReader source)
        {
            UnityEngine.Color32 data = (UnityEngine.Color32)value;

            int fieldNumber = 0;

            SubItemToken token = ProtoReader.StartSubItem(source);

            while ((fieldNumber = source.ReadFieldHeader()) != 0)
            {
                switch(fieldNumber)
                {
                    case 1:
                        data.r = source.ReadByte();
                        break;
                    case 2:
                        data.g = source.ReadByte();
                        break;
                    case 3:
                        data.b = source.ReadByte();
                        break;
                    case 4:
                        data.a = source.ReadByte();
                        break;
                    default:
                        source.SkipField();
                        break;
                }
            }

            ProtoReader.EndSubItem(token, source);

            return data;
        }

        public void Write(object value, ProtoWriter dest)
        {
            UnityEngine.Color32 data = (UnityEngine.Color32)value;

            SubItemToken token = ProtoWriter.StartSubItem(value, dest);

            ProtoWriter.WriteFieldHeader(1, WireType.Variant, dest);
            ProtoWriter.WriteByte(data.r, dest);

            ProtoWriter.WriteFieldHeader(2, WireType.Variant, dest);
            ProtoWriter.WriteByte(data.g, dest);

            ProtoWriter.WriteFieldHeader(3, WireType.Variant, dest);
            ProtoWriter.WriteByte(data.b, dest);

            ProtoWriter.WriteFieldHeader(4, WireType.Variant, dest);
            ProtoWriter.WriteByte(data.a, dest);

            ProtoWriter.EndSubItem(token, dest);
        }
    }
}

