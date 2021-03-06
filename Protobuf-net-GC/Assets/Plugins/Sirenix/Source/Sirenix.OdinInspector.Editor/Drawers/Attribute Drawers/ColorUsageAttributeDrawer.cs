#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ColorUsageAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws Color properties marked with <see cref="UnityEngine.ColorUsageAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class ColorUsageAttributeDrawer : OdinAttributeDrawer<ColorUsageAttribute, Color>, IDefinesGenericMenuItems
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<Color> entry, ColorUsageAttribute attribute, GUIContent label)
        {
            Rect rect = EditorGUILayout.GetControlRect(label != null);

#pragma warning disable 0612 // Type or member is obsolete
#pragma warning disable 0618 // Type or member is obsolete
            PropertyContext<ColorPickerHDRConfig> context;
            if (entry.Context.Get<ColorPickerHDRConfig>(this, "HdrConfig", out context))
            {
                context.Value = new ColorPickerHDRConfig(attribute.minBrightness, attribute.maxBrightness, attribute.minExposureValue, attribute.maxExposureValue);
            }
#pragma warning restore 0618 // Type or member is obsolete
#pragma warning restore 0612 // Type or member is obsolete

            //if (label != null)
            //{
            //    rect = EditorGUI.PrefixLabel(rect, label);
            //}

            bool disableContext = false;

            if (Event.current.OnMouseDown(rect, 1, false))
            {
                // Disable Unity's color field's own context menu
                GUIHelper.PushEventType(EventType.Used);
                disableContext = true;
            }

#pragma warning disable 0618 // Type or member is obsolete
            entry.SmartValue = EditorGUI.ColorField(rect, label ?? GUIContent.none, entry.SmartValue, true, attribute.showAlpha, attribute.hdr, context.Value);
#pragma warning restore 0618 // Type or member is obsolete

            if (disableContext)
            {
                GUIHelper.PopEventType();
            }
        }

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            ColorDrawer.PopulateGenericMenu((IPropertyValueEntry<Color>)property.ValueEntry, genericMenu);
        }
    }
}
#endif