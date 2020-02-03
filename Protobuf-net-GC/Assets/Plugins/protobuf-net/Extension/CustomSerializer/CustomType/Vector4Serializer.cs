using System;
using ProtoBuf.Meta;

namespace ProtoBuf.Extension
{
    sealed class Vector4Serializer : Serializers.IProtoTypeSerializer
    {
        public Type ExpectedType { get { return typeof(UnityEngine.Vector4); } }

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
            return new UnityEngine.Vector4();
        }

        public bool HasCallbacks(TypeModel.CallbackType callbackType)
        {
            return false;
        }

        public object Read(object value, ProtoReader source)
        {
            UnityEngine.Vector4 data = (UnityEngine.Vector4)value;

            int fieldNumber = 0;

            SubItemToken token = ProtoReader.StartSubItem(source);

            while ((fieldNumber = source.ReadFieldHeader()) != 0)
            {
                switch(fieldNumber)
                {
                    case 1:
                        data.x = source.ReadSingle();
                        break;
                    case 2:
                        data.y = source.ReadSingle();
                        break;
                    case 3:
                        data.z = source.ReadSingle();
                        break;
                    case 4:
                        data.w = source.ReadSingle();
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
            UnityEngine.Vector4 data = (UnityEngine.Vector4)value;

            SubItemToken token = ProtoWriter.StartSubItem(value, dest);

            ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, dest);
            ProtoWriter.WriteSingle(data.x, dest);

            ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, dest);
            ProtoWriter.WriteSingle(data.y, dest);

            ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, dest);
            ProtoWriter.WriteSingle(data.z, dest);

            ProtoWriter.WriteFieldHeader(4, WireType.Fixed32, dest);
            ProtoWriter.WriteSingle(data.w, dest);

            ProtoWriter.EndSubItem(token, dest);
        }
    }
}

