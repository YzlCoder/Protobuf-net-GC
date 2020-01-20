#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EnumSelector.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;

    /// <summary>
    /// A feature-rich enum selector with support for flag enums.
    /// </summary>
    /// <example>
    /// <code>
    /// KeyCode someEnumValue;
    /// 
    /// [OnInspectorGUI]
    /// void OnInspectorGUI()
    /// {
    ///     // Use the selector manually. See the documentation for OdinSelector for more information.
    ///     if (GUILayout.Button("Open Enum Selector"))
    ///     {
    ///         EnumSelector&lt;KeyCode&gt; selector = new EnumSelector&lt;KeyCode&gt;();
    ///         selector.SetSelection(this.someEnumValue);
    ///         selector.SelectionConfirmed += selection =&gt; this.someEnumValue = selection.FirstOrDefault();
    ///         selector.ShowInPopup(); // Returns the Odin Editor Window instance, in case you want to mess around with that as well.
    ///     }
    ///     
    ///     // Draw an enum dropdown field which uses the EnumSelector popup:
    ///     this.someEnumValue = EnumSelector&lt;KeyCode&gt;.DrawEnumField(new GUIContent("My Label"), this.someEnumValue);
    /// }
    /// 
    /// // All Odin Selectors can be rendered anywhere with Odin. This includes the EnumSelector.
    /// EnumSelector&lt;KeyCode&gt; inlineSelector;
    /// 
    /// [ShowInInspector]
    /// EnumSelector&lt;KeyCode&gt; InlineSelector
    /// {
    ///     get { return this.inlineSelector ?? (this.inlineSelector = new EnumSelector&lt;KeyCode&gt;()); }
    ///     set { }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="OdinSelector{T}"/>
    /// <seealso cref="TypeSelector"/>
    /// <seealso cref="GenericSelector{T}"/>
    /// <seealso cref="OdinMenuTree"/>
    /// <seealso cref="OdinEditorWindow"/>
    public class EnumSelector<T> : OdinSelector<T>
    {
        private static Color highlightLineColor = EditorGUIUtility.isProSkin ? new Color(0.5f, 1f, 0, 1f) : new Color(0.015f, 0.68f, 0.015f, 1f);
        private static Color selectedMaskBgColor = EditorGUIUtility.isProSkin ? new Color(0.5f, 1f, 0, 0.1f) : new Color(0.02f, 0.537f, 0, 0.31f);
        private static readonly List<object> enumValues;
        private static readonly bool isFlagEnum = typeof(T).HasCustomAttribute<FlagsAttribute>();
        private static readonly string title = typeof(T).Name.SplitPascalCase();
        private float maxEnumLabelWidth = 0;

        static EnumSelector()
        {
            if (typeof(T).IsEnum)
            {
                var memberNames = typeof(T).GetAllMembers(Flags.AllMembers).Select(x => x.Name).ToList();
                enumValues = Enum.GetNames(typeof(T)).OrderBy(x => memberNames.IndexOf(x)).Select(x => Enum.Parse(typeof(T), x)).ToList();
            }
        }

        private ulong curentValue;
        private ulong curentMouseOverValue;

        /// <summary>
        /// By default, the enum type will be drawn as the title for the selector. No title will be drawn if the string is null or empty.
        /// </summary>
        public override string Title
        {
            get
            {
                if (GeneralDrawerConfig.Instance.DrawEnumTypeTitle)
                {
                    return title;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is flag enum.
        /// </summary>
        public bool IsFlagEnum { get { return isFlagEnum; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumSelector{T}"/> class.
        /// </summary>
        public EnumSelector()
        {
            if (!typeof(T).IsEnum)
            {
                throw new NotSupportedException(typeof(T).GetNiceFullName() + " is not an enum type.");
            }

            if (Event.current != null)
            {
                foreach (var item in Enum.GetNames(typeof(T)))
                {
                    maxEnumLabelWidth = Mathf.Max(maxEnumLabelWidth, SirenixGUIStyles.Label.CalcSize(new GUIContent(item)).x);
                }

                if (this.Title != null)
                {
                    var titleAndSearch = Title + "                      ";
                    maxEnumLabelWidth = Mathf.Max(maxEnumLabelWidth, SirenixGUIStyles.Label.CalcSize(new GUIContent(titleAndSearch)).x);
                }
            }
        }

        /// <summary>
        /// Populates the tree with all enum values.
        /// </summary>
        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            tree.Selection.SupportsMultiSelect = isFlagEnum;
            tree.Config.DrawSearchToolbar = true;
            tree.AddRange(enumValues, x => Enum.GetName(typeof(T), x).SplitPascalCase());

            if (isFlagEnum)
            {
                tree.DefaultMenuStyle.Offset += 15;
                if (!enumValues.Select(x => Convert.ToInt64(x)).Contains(0))
                {
                    tree.MenuItems.Insert(0, new OdinMenuItem(tree, "None", 0));
                }
                tree.EnumerateTree().ForEach(x => x.OnDrawItem += DrawEnumFlagItem);
                this.DrawConfirmSelectionButton = false;
            }
            else
            {
                tree.EnumerateTree().ForEach(x => x.OnDrawItem += DrawEnumItem);
            }
        }

        private bool wasMouseDown = false;

        private void DrawEnumItem(OdinMenuItem obj)
        {
            if (Event.current.type == EventType.MouseDown && obj.Rect.Contains(Event.current.mousePosition))
            {
                obj.Select();
                Event.current.Use();
                wasMouseDown = true;
            }

            if (wasMouseDown)
            {
                GUIHelper.RequestRepaint();
            }

            if (wasMouseDown == true && Event.current.type == EventType.MouseDrag && obj.Rect.Contains(Event.current.mousePosition))
            {
                obj.Select();
            }

            if (Event.current.type == EventType.MouseUp)
            {
                wasMouseDown = false;
                if (obj.IsSelected && obj.Rect.Contains(Event.current.mousePosition))
                {
                    obj.MenuTree.Selection.ConfirmSelection();
                }
            }
        }

        [OnInspectorGUI, PropertyOrder(-1000)]
        private void SpaceToggleEnumFlag()
        {
            if (isFlagEnum && Event.current.keyCode == KeyCode.Space && Event.current.type == EventType.KeyDown && this.SelectionTree != null)
            {
                foreach (var item in this.SelectionTree.Selection)
                {
                    this.ToggleEnumFlag(item);
                }

                this.TriggerSelectionChanged();

                Event.current.Use();
            }
        }

        /// <summary>
        /// When ShowInPopup is called, without a specified window width, this method gets called.
        /// Here you can calculate and give a good default width for the popup.
        /// The default implementation returns 0, which will let the popup window determine the width itself. This is usually a fixed value.
        /// </summary>
        protected override float DefaultWindowWidth()
        {
            return Mathf.Clamp(maxEnumLabelWidth + 50, 160, 400);
        }

        private void DrawEnumFlagItem(OdinMenuItem obj)
        {
            if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp) && obj.Rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    ToggleEnumFlag(obj);

                    this.TriggerSelectionChanged();
                }
                Event.current.Use();
            }

            if (Event.current.type == EventType.Repaint)
            {
                var val = (ulong)Convert.ToInt64(obj.ObjectInstance);
                var isPowerOfTwo = (val & (val - 1)) == 0;

                if (val != 0 && !isPowerOfTwo)
                {
                    var isMouseOver = obj.Rect.Contains(Event.current.mousePosition);
                    if (isMouseOver)
                    {
                        curentMouseOverValue = val;
                    }
                    else if (val == curentMouseOverValue)
                    {
                        curentMouseOverValue = 0;
                    }
                }

                var chked = (val & this.curentValue) == val && !((val == 0 && this.curentValue != 0));
                var highlight = val != 0 && isPowerOfTwo && (val & this.curentMouseOverValue) == val && !((val == 0 && this.curentMouseOverValue != 0));

                if (highlight)
                {
                    EditorGUI.DrawRect(obj.Rect.AlignLeft(6).Padding(2), highlightLineColor);
                }

                if (chked || isPowerOfTwo)
                {
                    var rect = obj.Rect.AlignLeft(30).AlignCenter(EditorIcons.TestPassed.width, EditorIcons.TestPassed.height);
                    if (chked)
                    {
                        if (isPowerOfTwo)
                        {
                            if (!EditorGUIUtility.isProSkin)
                            {
                                var tmp = GUI.color;
                                GUI.color = new Color(1, 0.7f, 1, 1);
                                GUI.DrawTexture(rect, EditorIcons.TestPassed);
                                GUI.color = tmp;
                            }
                            else
                            {
                                GUI.DrawTexture(rect, EditorIcons.TestPassed);
                            }

                        }
                        else
                        {
                            EditorGUI.DrawRect(obj.Rect.AlignTop(obj.Rect.height - (EditorGUIUtility.isProSkin ? 1 : 0)), selectedMaskBgColor);
                        }
                    }
                    else
                    {
                        GUI.DrawTexture(rect, EditorIcons.TestNormal);
                    }
                }
            }
        }

        private void ToggleEnumFlag(OdinMenuItem obj)
        {
            var val = (ulong)Convert.ToInt64(obj.ObjectInstance);
            if ((val & this.curentValue) == val)
            {
                this.curentValue = val == 0 ? 0 : (this.curentValue & ~val);
            }
            else
            {
                this.curentValue = this.curentValue | val;
            }

            if (Event.current.clickCount >= 2)
            {
                Event.current.Use();
            }
        }

        /// <summary>
        /// Gets the currently selected enum value.
        /// </summary>
        public override IEnumerable<T> GetCurrentSelection()
        {
            if (isFlagEnum)
            {
                yield return (T)Enum.ToObject(typeof(T), this.curentValue);
            }
            else
            {
                if (this.SelectionTree.Selection.Count > 0)
                {
                    yield return (T)Enum.ToObject(typeof(T), this.SelectionTree.Selection.Last().ObjectInstance);
                }
            }
        }

        /// <summary>
        /// Selects an enum.
        /// </summary>
        public override void SetSelection(T selected)
        {
            if (isFlagEnum)
            {
                this.curentValue = (ulong)Convert.ToInt64(selected);
            }
            else
            {
                var selection = this.SelectionTree.EnumerateTree().Where(x => Convert.ToInt64(x.ObjectInstance) == Convert.ToInt64(selected));
                this.SelectionTree.Selection.AddRange(selection);
            }
        }
        /// <summary>
        /// Draws an enum selector field using the enum selector.
        /// </summary>
        public static T DrawEnumField(GUIContent label, T value)
        {
            int id;
            bool hasFocus;
            Rect rect;
            Action<EnumSelector<T>> bindSelector;
            Func<IEnumerable<T>> getResult;

            var display = (isFlagEnum && Convert.ToInt64(value) == 0) ? "None" : (EditorGUI.showMixedValue ? "—" : value.ToString());
            TempFeatureRichControlRect(label, out id, out hasFocus, out rect);

            if (DrawSelectorButton(rect, display, EditorStyles.popup, id, out bindSelector, out getResult))
            {
                var selector = new EnumSelector<T>();

                if (!EditorGUI.showMixedValue)
                {
                    selector.SetSelection(value);
                }

                var window = selector.ShowInPopup(new Vector2(rect.xMin, rect.yMax));

                if (isFlagEnum)
                {
                    window.OnClose += selector.SelectionTree.Selection.ConfirmSelection;
                }

                bindSelector(selector);
            }

            if (getResult != null)
            {
                value = getResult().FirstOrDefault();
                GUI.changed = true;
            }

            return value;
        }
        
        private static Rect TempFeatureRichControlRect(GUIContent label, out int controlId, out bool hasFocus, out Rect valueRect)
        {
            Rect totalRect;

            valueRect = EditorGUILayout.GetControlRect(label != null);
            controlId = GUIUtility.GetControlID(FocusType.Keyboard);

            if (label == null)
            {
                valueRect = EditorGUI.IndentedRect(valueRect);
                totalRect = valueRect;
            }
            else
            {
                totalRect = valueRect;
                totalRect.xMin += EditorGUI.indentLevel * 15f;
                valueRect = EditorGUI.PrefixLabel(valueRect, controlId, label);
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && valueRect.Contains(Event.current.mousePosition))
            {
                GUIUtility.keyboardControl = controlId;
            }

            hasFocus = GUIUtility.keyboardControl == controlId && GUIHelper.CurrentWindow == EditorWindow.focusedWindow;
            return totalRect;
        }
    }
}
#endif