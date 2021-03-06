#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideInPlayModeAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="HideInPlayModeAttribute"/>
    /// </summary>
    /// <seealso cref="HideInInspector"/>
    /// <seealso cref="ShowIfAttribute"/>
    /// <seealso cref="HideIfAttribute"/>
    /// <seealso cref="ReadOnlyAttribute"/>
    /// <seealso cref="EnableIfAttribute"/>
    /// <seealso cref="DisableIfAttribute"/>
    /// <seealso cref="DisableInEditorModeAttribute"/>
    /// <seealso cref="DisableInPlayModeAttribute"/>
    [OdinDrawer]
    [DrawerPriority(100, 0, 0)]
    public sealed class HideInPlayModeAttributeDrawer : OdinAttributeDrawer<HideInPlayModeAttribute>
    {
        /// <summary>
        /// Does not call the next drawer, when the editor is in play mode.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, HideInPlayModeAttribute attribute, GUIContent label)
        {
            if (!Application.isPlaying)
            {
                this.CallNextDrawer(property, label);
            }
        }
    }
}
#endif