using System;
using ProtoBuf.Meta;

namespace ProtoBuf.Extension
{
    sealed class ColorSerializer : Serializers.IProtoTypeSerializer
    {
        public Type ExpectedType { get { return typeof(UnityEngine.Color); } }

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
            return new UnityEngine.Color();
        }

        public bool HasCallbacks(TypeModel.CallbackType callbackType)
        {
            return false;
        }

        public object Read(object value, ProtoReader source)
        {
            UnityEngine.Color data = (UnityEngine.Color)value;

            int fieldNumber = 0;

            SubItemToken token = ProtoReader.StartSubItem(source);

            while ((fieldNumber = source.ReadFieldHeader()) != 0)
            {
                switch(fieldNumber)
                {
                    case 1:
                        data.r = source.ReadSingle();
                        break;
                    case 2:
                        data.g = source.ReadSingle();
                        break;
                    case 3:
                        data.b = source.ReadSingle();
                        break;
                    case 4:
                        data.a = source.ReadSingle();
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
            UnityEngine.Color data = (UnityEngine.Color)value;

            SubItemToken token = ProtoWriter.StartSubItem(value, dest);

            ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, dest);
            ProtoWriter.WriteSingle(data.r, dest);

            ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, dest);
            ProtoWriter.WriteSingle(data.g, dest);

            ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, dest);
            ProtoWriter.WriteSingle(data.b, dest);

            ProtoWriter.WriteFieldHeader(4, WireType.Fixed32, dest);
            ProtoWriter.WriteSingle(data.a, dest);

            ProtoWriter.EndSubItem(token, dest);
        }
    }
}

