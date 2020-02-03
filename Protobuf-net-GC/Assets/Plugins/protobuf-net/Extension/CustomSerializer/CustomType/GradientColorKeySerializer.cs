using System;
using ProtoBuf.Meta;
using ProtoBuf.Serializers;

namespace ProtoBuf.Extension
{
    sealed class GradientColorKeySerializer : Serializers.IProtoTypeSerializer
    {
        public Type ExpectedType { get { return typeof(UnityEngine.GradientColorKey); } }

        public bool RequiresOldValue { get { return true; } }

        public bool ReturnsValue { get { return true; } }

        private ColorSerializer colorSerializer = null;

        public GradientColorKeySerializer()
        {
            this.colorSerializer = new ColorSerializer();
        }


        public void Callback(object value, TypeModel.CallbackType callbackType, SerializationContext context)
        {
            
        }

        public bool CanCreateInstance()
        {
            return true;
        }

        public object CreateInstance(ProtoReader source)
        {
            return new UnityEngine.GradientColorKey();
        }

        public bool HasCallbacks(TypeModel.CallbackType callbackType)
        {
            return false;
        }

        public object Read(object value, ProtoReader source)
        {
            UnityEngine.GradientColorKey data = (UnityEngine.GradientColorKey)value;

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
                        data.color = (UnityEngine.Color)this.colorSerializer.Read(data.color, source);
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
            UnityEngine.GradientColorKey data = (UnityEngine.GradientColorKey)value;

            SubItemToken token = ProtoWriter.StartSubItem(value, dest);

            ProtoWriter.WriteFieldHeader(1, WireType.Fixed32, dest);
            ProtoWriter.WriteSingle(data.time, dest);

            ProtoWriter.WriteFieldHeader(2, WireType.String , dest);
            this.colorSerializer.Write(data.color, dest);

            ProtoWriter.EndSubItem(token, dest);
        }
    }
}

