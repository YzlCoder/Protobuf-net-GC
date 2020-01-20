
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;


public static partial class ProtoSerialize
{
    
    
    private static class ProtoSerializeReg<T> where T : new()
    {
        public delegate T DeserializeFun(Stream stream);
        public delegate T DeserializeFunInner(ProtoReader source, T defaultVal);
    
        private static readonly DeserializeFun _default = stream => Serializer.Deserialize<T>(stream);
        private static DeserializeFunInner _invoke = null;
        public static DeserializeFunInner Invoke
        {
            set { _invoke = value; }
        }
        public static T Deserialize(Stream stream, T obj) 
        {
            if (_invoke == null)
            {
                return _default(stream);
            }

            obj = ProtoSerializeHelper<T>.CreateInstance(obj);
		
            ProtoReader source = null;
            try
            {
                source = ProtoReader.Create(stream, RuntimeTypeModel.Default, null, ProtoReader.TO_EOF);
                source.SetRootObject(obj);
                obj = _invoke(source, obj);
                source.CheckFullyConsumed();
                return obj;
            }
            finally
            {
                ProtoReader.Recycle(source);
            }
        }
    }
    
    public static T Deserialize<T>(Stream stream, T obj) where T : new()
    {
        return ProtoSerializeReg<T>.Deserialize(stream, obj);
    }
}

