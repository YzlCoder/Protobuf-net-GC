using System;
using ProtoBuf.Meta;
using ProtoBuf.Serializers;

namespace ProtoBuf.Extension
{
    sealed class KeyframeSerializer : Serializers.IProtoTypeSerializer
    {
        public Type ExpectedType { get { return typeof(UnityEngine.Keyframe); } }

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
            return new UnityEngine.Keyframe();
        }

        public bool HasCallbacks(TypeModel.CallbackType callbackType)
        {
            return false;
        }

        public object Read(object value, ProtoReader source)
        {
            UnityEngine.Keyframe data = (UnityEngine.Keyframe)value;

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
                        data.value = source.ReadSingle();
                        break;
                    case 3:
                        data.inTangent = source.ReadSingle();
                        break;
                    case 4:
                        data.outTangent = source.ReadSingle();
                        break;
                    case 5:
                        data.tangentMode = source.ReadInt32();
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
            UnityEngine.Keyframe data = (UnityEngine.Keyframe)value;

            SubItemToken token = ProtoWriter.StartSubItem(value, dest);

            ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, dest);
            ProtoWriter.WriteSingle(data.time, dest);

            ProtoWriter.WriteFieldHeader(2, WireType.Fixed32, dest);
            ProtoWriter.WriteSingle(data.value, dest);

            ProtoWriter.WriteFieldHeader(3, WireType.Fixed32, dest);
            ProtoWriter.WriteSingle(data.inTangent, dest);

            ProtoWriter.WriteFieldHeader(4, WireType.Fixed32, dest);
            ProtoWriter.WriteSingle(data.outTangent, dest);

            ProtoWriter.WriteFieldHeader(5, WireType.Variant, dest);
            ProtoWriter.WriteInt32(data.tangentMode, dest);

            ProtoWriter.EndSubItem(token, dest);
        }
    }
}

