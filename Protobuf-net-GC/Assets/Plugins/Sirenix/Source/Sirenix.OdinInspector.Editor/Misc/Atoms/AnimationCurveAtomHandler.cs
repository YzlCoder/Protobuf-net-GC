#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AnimationCurveAtomHandler.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
    using System.Collections.Generic;
    using UnityEngine;

    [AtomHandler]
    public sealed class AnimationCurveAtomHandler : BaseAtomHandler<AnimationCurve>
    {
        public override bool NeedUpdateValue()
        {
            return false;
        }

        public override AnimationCurve CreateInstance()
        {
            return new AnimationCurve();
        }

        protected override bool CompareImplementation(AnimationCurve a, AnimationCurve b)
        {
            if (a.postWrapMode != b.postWrapMode || a.preWrapMode != b.preWrapMode)
            {
                return false;
            }

            int aLength = a.length;
            int bLength = b.length;
            if (aLength != bLength)
            {
                // 帧数是否相同
                return false;
            }

            if (aLength == 0)
            {
                // 都没有关键帧
                return true;
            }
            
            var aKeyArrary = a.keys;
            var bKeyArrary = b.keys;

            for (int i = 0; i < aLength; i++)
            {
                var aKey = aKeyArrary[i];
                var bKey = bKeyArrary[i];

                if (!EqualityComparer<Keyframe>.Default.Equals(aKey, bKey))
                {
                    return false;
                }
            }

            return true;
        }

        protected override void CopyImplementation(ref AnimationCurve from, ref AnimationCurve to)
        {
            to.postWrapMode = from.postWrapMode;
            to.preWrapMode = from.preWrapMode;

            // src curve length
            int fromLength = from.length;

            // 长度保证一致
            while (to.length > fromLength)
            {
                to.RemoveKey(to.length - 1);
            }

            while (to.length < fromLength)
            {
                // Just a random value, as it'll be set further down; adding the same time values several times does nothing
                to.AddKey(UnityEngine.Random.Range(0f, 1f), 0f);
            }

            if (to.length > 0)
            {
                // 只剩下一次GC Alloc
                var fromKeys = from.keys;
                for (int i = 0; i < to.length; i++)
                {
                    to.MoveKey(i, fromKeys[i]);
                }
            }
        }
    }
}
#endif