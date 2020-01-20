using UnityEngine;
using UnityEngine.Scripting;

namespace Sirenix.Serialization.AOTGenerated
{
    [Preserve]
    internal static class PreventCodeStrippingViaReferences
    {
        static PreventCodeStrippingViaReferences()
        {
            Bounds bounds = new Bounds();
            BoundsFormatter boundsFormatter = new BoundsFormatter();
            ReflectionFormatter<Bounds> reflectionFormatter1 = new ReflectionFormatter<Bounds>();
            ComplexTypeSerializer<Bounds> complexTypeSerializer1 = new ComplexTypeSerializer<Bounds>();
            
            Color color = new Color();
            ColorFormatter colorFormatter = new ColorFormatter();
            ReflectionFormatter<Color> reflectionFormatter3 = new ReflectionFormatter<Color>();
            ComplexTypeSerializer<Color> complexTypeSerializer3 = new ComplexTypeSerializer<Color>();
            
            Matrix4x4 matrix4x4 = new Matrix4x4();
            ReflectionFormatter<Matrix4x4> reflectionFormatter4 = new ReflectionFormatter<Matrix4x4>();
            ComplexTypeSerializer<Matrix4x4> complexTypeSerializer4 = new ComplexTypeSerializer<Matrix4x4>();
            
            Quaternion quaternion = new Quaternion();
            QuaternionFormatter quaternionFormatter = new QuaternionFormatter();
            ReflectionFormatter<Quaternion> reflectionFormatter5 = new ReflectionFormatter<Quaternion>();
            ComplexTypeSerializer<Quaternion> complexTypeSerializer5 = new ComplexTypeSerializer<Quaternion>();
            
            Rect rect = new Rect();
            RectFormatter rectFormatter = new RectFormatter();
            ReflectionFormatter<Rect> reflectionFormatter6 = new ReflectionFormatter<Rect>();
            ComplexTypeSerializer<Rect> complexTypeSerializer6 = new ComplexTypeSerializer<Rect>();
            
            Vector2 vector2 = new Vector2();
            Vector2Formatter vector2Formatter = new Vector2Formatter();
            ReflectionFormatter<Vector2> reflectionFormatter8 = new ReflectionFormatter<Vector2>();
            ComplexTypeSerializer<Vector2> complexTypeSerializer8 = new ComplexTypeSerializer<Vector2>();
            
            Vector3 vector3 = new Vector3();
            Vector3Formatter vector3Formatter = new Vector3Formatter();
            ReflectionFormatter<Vector3> reflectionFormatter10 = new ReflectionFormatter<Vector3>();
            ComplexTypeSerializer<Vector3> complexTypeSerializer10 = new ComplexTypeSerializer<Vector3>();
            
            Vector4 vector4 = new Vector4();
            Vector4Formatter vector4Formatter = new Vector4Formatter();
            ReflectionFormatter<Vector4> reflectionFormatter12 = new ReflectionFormatter<Vector4>();
            ComplexTypeSerializer<Vector4> complexTypeSerializer12 = new ComplexTypeSerializer<Vector4>();
        }
    }
}