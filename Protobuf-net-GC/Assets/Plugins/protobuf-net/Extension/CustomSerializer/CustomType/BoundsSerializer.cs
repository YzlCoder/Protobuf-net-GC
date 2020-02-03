using System;
using ProtoBuf.Meta;
using ProtoBuf.Serializers;

namespace ProtoBuf.Extension
{
    sealed class BoundsSerializer : Serializers.IProtoTypeSerializer
    {
        public Type ExpectedType { get { return typeof(UnityEngine.Bounds); } }

        public bool RequiresOldValue { get { return true; } }

        public bool ReturnsValue { get { return true; } }

        private IProtoSerializer vector3Serializer = null;

        public BoundsSerializer()
        {
            this.vector3Serializer = new Vector3Serializer();
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
            return new UnityEngine.Bounds();
        }

        public bool HasCallbacks(TypeModel.CallbackType callbackType)
        {
            return false;
        }

        public object Read(object value, ProtoReader source)
        {
            UnityEngine.Bounds data = (UnityEngine.Bounds)value;

            int fieldNumber = 0;

            SubItemToken token = ProtoReader.StartSubItem(source);

            while ((fieldNumber = source.ReadFieldHeader()) != 0)
            {
                switch(fieldNumber)
                {
                    case 1:
                        data.center = (UnityEngine.Vector3)this.vector3Serializer.Read(data.center, source);
                        break;
                    case 2:
                        data.size = (UnityEngine.Vector3)this.vector3Serializer.Read(data.size, source);
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
            UnityEngine.Bounds data = (UnityEngine.Bounds)value;

            SubItemToken token = ProtoWriter.StartSubItem(value, dest);

            ProtoWriter.WriteFieldHeader(1, WireType.String, dest);
            this.vector3Serializer.Write(data.center, dest);

            ProtoWriter.WriteFieldHeader(2, WireType.String, dest);
            this.vector3Serializer.Write(data.size, dest);

            ProtoWriter.EndSubItem(token, dest);
        }
    }
}

