#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinMenuTree.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using UnityEngine;

    [OdinDrawer]
    internal class OdinMenuTreeDrawer : OdinValueDrawer<OdinMenuTree>
    {
        protected override void DrawPropertyLayout(IPropertyValueEntry<OdinMenuTree> entry, GUIContent label)
        {
            var tree = entry.SmartValue;
            if (tree != null)
            {
                tree.DrawMenuTree();
                tree.HandleKeybaordMenuNavigation();
            }
        }
    }
}
#endif