using ProtoBuf.Serializers;
using System;
using System.Collections.Generic;

namespace ProtoBuf.Extension
{
    static partial class CustomSerializerReg
    {
        private static Dictionary<Type, IProtoTypeSerializer> allocaters = 
            new Dictionary<Type, IProtoTypeSerializer>();
        
        static CustomSerializerReg()
        {
            AddCustomSerializer(typeof(UnityEngine.Vector2), new Vector2Serializer());
            AddCustomSerializer(typeof(UnityEngine.Vector3), new Vector3Serializer());
            AddCustomSerializer(typeof(UnityEngine.Vector4), new Vector4Serializer());
            AddCustomSerializer(typeof(UnityEngine.Color), new ColorSerializer());
            AddCustomSerializer(typeof(UnityEngine.Color32), new Color32Serializer());
            AddCustomSerializer(typeof(UnityEngine.Bounds), new BoundsSerializer());
            AddCustomSerializer(typeof(UnityEngine.Quaternion), new QuaternionSerializer());
            AddCustomSerializer(typeof(UnityEngine.LayerMask), new LayerMaskSerializer());
            AddCustomSerializer(typeof(UnityEngine.Rect), new RectSerializer());
            AddCustomSerializer(typeof(UnityEngine.Keyframe), new KeyframeSerializer());
            AddCustomSerializer(typeof(UnityEngine.AnimationCurve), new AnimationCurveSerializer());
            AddCustomSerializer(typeof(UnityEngine.GradientAlphaKey), new GradientAlphaKeySerializer());
            AddCustomSerializer(typeof(UnityEngine.GradientColorKey), new GradientColorKeySerializer());
            AddCustomSerializer(typeof(UnityEngine.Gradient), new GradientSerializer());
        }

        public static bool TryGetSerializer(Type targetType, ref IProtoSerializer ser, ref WireType wireType)
        {
            bool isUnityObjectType = targetType.IsSubclassOf(typeof(UnityEngine.Object));
            if (allocaters.ContainsKey(targetType))
            {
                ser = allocaters[targetType];
                if(ser != null)
                {
                    if(isUnityObjectType)
                    {
                        wireType = WireType.Variant;
                    }
                    else
                    {
                        wireType = WireType.String;
                    }
                    
                }
                return ser != null;
            }

            if(isUnityObjectType)
            {
                ser = Activator.CreateInstance(typeof(UnityRefObjectSerializer<>).MakeGenericType(targetType)) 
                    as IProtoTypeSerializer;
                if (ser != null)
                {
                    AddCustomSerializer(targetType, ser as IProtoTypeSerializer);
                    wireType = WireType.Variant;
                    return ser != null;
                }
            }
            return false;
        }

        public static void AddCustomSerializer(Type type, IProtoTypeSerializer customSerializer)
        {
            if (allocaters.ContainsKey(type))
            {
                //exist
                return;
            }
            allocaters.Add(type, customSerializer);
        }

       
    }
}
