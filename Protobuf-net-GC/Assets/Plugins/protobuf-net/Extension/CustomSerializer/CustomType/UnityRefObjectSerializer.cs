using System;
using ProtoBuf.Meta;
using ProtoBuf.Serializers;

namespace ProtoBuf.Extension
{
    sealed class UnityRefObjectSerializer<T> : Serializers.IProtoTypeSerializer where T : UnityEngine.Object
    {
        public Type ExpectedType { get { return typeof(T); } }

        public bool RequiresOldValue { get { return true; } }

        public bool ReturnsValue { get { return true; } }


        public UnityRefObjectSerializer()
        {

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
            return null;
        }

        public bool HasCallbacks(TypeModel.CallbackType callbackType)
        {
            return false;
        }

        public object Read(object value, ProtoReader source)
        {
            UnityEngine.Object data = ProtoReader.ReadUnityObject(source);

            return data;
        }

        public void Write(object value, ProtoWriter dest)
        {
            ProtoWriter.WriteUnityObject(dest, (UnityEngine.Object)value);
        }
    }
}

