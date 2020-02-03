using System;
using ProtoBuf.Meta;
using ProtoBuf.Serializers;

namespace ProtoBuf.Extension
{
    sealed class GradientAlphaKeySerializer : Serializers.IProtoTypeSerializer
    {
        public Type ExpectedType { get { return typeof(UnityEngine.GradientAlphaKey); } }

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
            return new UnityEngine.GradientAlphaKey();
        }

        public bool HasCallbacks(TypeModel.CallbackType callbackType)
        {
            return false;
        }

        public object Read(object value, ProtoReader source)
        {
            UnityEngine.GradientAlphaKey data = (UnityEngine.GradientAlphaKey)value;

            int fieldNumber = 0;

            SubItemToken token = ProtoReader.StartSubItem(source);

            while ((fieldNumber = source.ReadFieldHeader()) != 0)
            {
                switch(fieldNumber)
                {
                    case 1:
                        data.time = source.ReadSingle();
                        break;
                    case 2:
                        data.alpha = source.ReadSingle();
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
            UnityEngine.GradientAlphaKey data = (UnityEngine.GradientAlphaKey)value;

            SubItemToken token = ProtoWriter.StartSubItem(value, dest);

            ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, dest);
            ProtoWriter.WriteSingle(data.time, dest);

            ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, dest);
            ProtoWriter.WriteSingle(data.alpha, dest);

            ProtoWriter.EndSubItem(token, dest);
        }
    }
}

