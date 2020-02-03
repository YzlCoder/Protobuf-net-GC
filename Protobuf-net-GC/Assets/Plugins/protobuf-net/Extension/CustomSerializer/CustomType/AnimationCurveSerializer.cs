using System;
using ProtoBuf.Meta;
using ProtoBuf.Serializers;
using System.Collections.Generic;

namespace ProtoBuf.Extension
{
    sealed class AnimationCurveSerializer : Serializers.IProtoTypeSerializer
    {
        public Type ExpectedType { get { return typeof(UnityEngine.AnimationCurve); } }

        public bool RequiresOldValue { get { return true; } }

        public bool ReturnsValue { get { return true; } }

        private KeyframeSerializer keyframeSerializer = null;
        private List<UnityEngine.Keyframe> keys = new List<UnityEngine.Keyframe>();

        public AnimationCurveSerializer()
        {
            this.keyframeSerializer = new KeyframeSerializer();
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
            return new UnityEngine.AnimationCurve();
        }

        public bool HasCallbacks(TypeModel.CallbackType callbackType)
        {
            return false;
        }

        public object Read(object value, ProtoReader source)
        {
            UnityEngine.AnimationCurve data = (UnityEngine.AnimationCurve)
                (value == null ? CreateInstance(source) : value);

            int fieldNumber = 0;

            SubItemToken token = ProtoReader.StartSubItem(source);

            while ((fieldNumber = source.ReadFieldHeader()) != 0)
            {
                switch(fieldNumber)
                {
                    case 1:
                        data.preWrapMode = (UnityEngine.WrapMode)source.ReadInt32();
                        break;
                    case 2:
                        data.postWrapMode = (UnityEngine.WrapMode)source.ReadInt32();
                        break;
                    case 3:
                        keys.Clear();
                        do
                        {
                            keys.Add((UnityEngine.Keyframe)this.keyframeSerializer.Read(
                                new UnityEngine.Keyframe(), source));
                        } while (source.TryReadFieldHeader(3));
                        data.keys = keys.ToArray();
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
            UnityEngine.AnimationCurve data = (UnityEngine.AnimationCurve)value;

            SubItemToken token = ProtoWriter.StartSubItem(value, dest);

            ProtoWriter.WriteFieldHeader(1, WireType.Variant, dest);
            ProtoWriter.WriteInt32((int)data.preWrapMode, dest);

            ProtoWriter.WriteFieldHeader(2, WireType.Variant, dest);
            ProtoWriter.WriteInt32((int)data.postWrapMode, dest);

            for(int i = 0; i < data.keys.Length; ++i)
            {
                ProtoWriter.WriteFieldHeader(3, WireType.String, dest);
                this.keyframeSerializer.Write(data.keys[i], dest);
            }


            ProtoWriter.EndSubItem(token, dest);
        }
    }
}

