using System;
using ProtoBuf.Meta;
using ProtoBuf.Serializers;
using System.Collections.Generic;

namespace ProtoBuf.Extension
{
    sealed class GradientSerializer : Serializers.IProtoTypeSerializer
    {
        public Type ExpectedType { get { return typeof(UnityEngine.Gradient); } }

        public bool RequiresOldValue { get { return true; } }

        public bool ReturnsValue { get { return true; } }

        private List<UnityEngine.GradientAlphaKey> alphaKeys = new List<UnityEngine.GradientAlphaKey>();
        private List<UnityEngine.GradientColorKey> colorKeys = new List<UnityEngine.GradientColorKey>();

        private GradientColorKeySerializer colorKeySerializer = null;
        private GradientAlphaKeySerializer alphaKeySerializer = null;


        public GradientSerializer()
        {
            this.colorKeySerializer = new GradientColorKeySerializer();
            this.alphaKeySerializer = new GradientAlphaKeySerializer();
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
            return new UnityEngine.Gradient();
        }

        public bool HasCallbacks(TypeModel.CallbackType callbackType)
        {
            return false;
        }

        public object Read(object value, ProtoReader source)
        {
            UnityEngine.Gradient data = (UnityEngine.Gradient)(value == null ? CreateInstance(source) : value);

            int fieldNumber = 0;

            SubItemToken token = ProtoReader.StartSubItem(source);

            while ((fieldNumber = source.ReadFieldHeader()) != 0)
            {
                switch(fieldNumber)
                {
                    case 1:
                        data.mode = (UnityEngine.GradientMode)source.ReadInt32();
                        break;
                    case 2:
                        alphaKeys.Clear();
                        do
                        {
                            alphaKeys.Add((UnityEngine.GradientAlphaKey)this.alphaKeySerializer.Read(
                                new UnityEngine.GradientAlphaKey(), source));
                        } while (source.TryReadFieldHeader(2));
                        data.alphaKeys = alphaKeys.ToArray();
                        break;
                    case 3:
                        colorKeys.Clear();
                        do
                        {
                            colorKeys.Add((UnityEngine.GradientColorKey)this.colorKeySerializer.Read(
                                new UnityEngine.GradientColorKey(), source));
                        } while (source.TryReadFieldHeader(3));
                        data.colorKeys = colorKeys.ToArray();
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
            UnityEngine.Gradient data = (UnityEngine.Gradient)value;

            SubItemToken token = ProtoWriter.StartSubItem(value, dest);

            ProtoWriter.WriteFieldHeader(1, WireType.Variant, dest);
            ProtoWriter.WriteInt32((int)data.mode, dest);

            for(int i = 0; i < data.alphaKeys.Length; ++i)
            {
                ProtoWriter.WriteFieldHeader(2, WireType.String, dest);
                this.alphaKeySerializer.Write(data.alphaKeys[i], dest);
            }

            for (int i = 0; i < data.colorKeys.Length; ++i)
            {
                ProtoWriter.WriteFieldHeader(3, WireType.String, dest);
                this.colorKeySerializer.Write(data.colorKeys[i], dest);
            }

            ProtoWriter.EndSubItem(token, dest);
        }
    }
}

