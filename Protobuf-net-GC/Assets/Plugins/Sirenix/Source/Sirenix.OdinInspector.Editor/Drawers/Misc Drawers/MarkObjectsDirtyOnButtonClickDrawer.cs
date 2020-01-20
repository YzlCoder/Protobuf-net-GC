#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MarkObjectsDirtyOnButtonClickDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Ensures all buttons are marked dirty when clicked. This can be customized in the GeneralDrawerConfig.
    /// </summary>
    [OdinDrawer]
    [DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
    internal class MarkObjectsDirtyOnButtonClickDrawer : Sirenix.OdinInspector.Editor.OdinAttributeDrawer<ButtonAttribute>
    {
        protected override void DrawPropertyLayout(InspectorProperty property, ButtonAttribute attribute, GUIContent label)
        {
            if (GeneralDrawerConfig.Instance.MarkObjectsDirtyOnButtonClick)
            {
                EditorGUI.BeginChangeCheck();
                this.CallNextDrawer(property, label);
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (var target in property.Tree.WeakTargets.OfType<UnityEngine.Object>())
                    {
                        EditorUtility.SetDirty(target);
                    }
                }
            }
            else
            {
                this.CallNextDrawer(property, label);
            }
        }
    }
}
#endif