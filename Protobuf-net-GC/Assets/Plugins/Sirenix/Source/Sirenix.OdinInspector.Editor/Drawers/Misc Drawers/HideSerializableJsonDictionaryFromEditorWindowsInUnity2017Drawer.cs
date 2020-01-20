#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideSerializableJsonDictionaryFromEditorWindowsInUnity2017Drawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using System;
    using UnityEngine;
    using Sirenix.OdinInspector.Editor;

    [OdinDrawer]
    [DrawerPriority(9001, 0, 0)]
    internal class HideSerializableJsonDictionaryFromEditorWindowsInUnity2017Drawer<T> : OdinValueDrawer<T> where T : ScriptableObject
    {
        public override bool CanDrawTypeFilter(Type type)
        {
            return type.FullName == "UnityEditor.Experimental.UIElements.SerializableJsonDictionary";
        }

        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, GUIContent label)
        {
            var member = entry.Property.Info.MemberInfo;
            if (member.MemberType == System.Reflection.MemberTypes.Field && member.Name == "m_PersistentViewDataDictionary")
            {
                return;
            }

            this.CallNextDrawer(entry, label);
        }
    }
}
#endif