#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TypeSelector.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
    using System;
    using System.Collections.Generic;
    using Sirenix.Utilities;
    using System.Linq;
    using UnityEngine;
    using Sirenix.Utilities.Editor;
    using UnityEditor;

    /// <summary>
    /// A feature-rich type selector.
    /// </summary>
    /// <example>
    /// <code>
    /// Type[] selectedTypes;
    /// 
    /// void OnGUI()
    /// {
    ///     // Use the selector manually. See the documentation for OdinSelector for more information.
    ///     if (GUILayout.Button("Open My Selector"))
    ///     {
    ///         TypeSelector selector = new TypeSelector(customListOfTypes);
    ///         TypeSelector selector = new TypeSelector(AssemblyTypeFlags.CustomTypes, supportsMultiSelect: true);
    ///         selector.SetSelection(this.selectedTypes);
    ///         selector.SelectionConfirmed += selection =&gt; this.selectedTypes = selection.ToArray();
    ///         selector.ShowInPopup(); // Returns the Odin Editor Window instance, in case you want to mess around with that as well.
    ///     }
    /// }
    /// </code>
    /// </example>
    public class TypeSelector : OdinSelector<Type>
    {
        private static Dictionary<AssemblyTypeFlags, List<OdinMenuItem>> cachedAllTypesMenuItems = new Dictionary<AssemblyTypeFlags, List<OdinMenuItem>>();
        private IEnumerable<Type> types;
        private AssemblyTypeFlags assemblyTypeFlags;
        private bool supportsMultiSelect;

        public override string Title { get { return null; /* "Select Type";*/ } }

        public TypeSelector(AssemblyTypeFlags assemblyFlags, bool supportsMultiSelect)
        {
            this.types = null;
            this.supportsMultiSelect = supportsMultiSelect;
            this.assemblyTypeFlags = assemblyFlags;
        }

        public TypeSelector(IEnumerable<Type> types, bool supportsMultiSelect)
        {
            this.types = types != null ? OrderTypes(types) : types;
            this.supportsMultiSelect = supportsMultiSelect;
        }

        private static IEnumerable<Type> OrderTypes(IEnumerable<Type> types)
        {
            return types.OrderByDescending(x => x.Namespace.IsNullOrWhitespace())
                                .ThenBy(x => x.Namespace)
                                .ThenBy(x => x.Name);
        }

        public override bool IsValidSelection(IEnumerable<Type> collection)
        {
            return collection.Any();
        }

        /// <summary>
        /// Builds the selection tree.
        /// </summary>
        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            tree.DefaultMenuStyle.NotSelectedIconAlpha = 1f;
            if (types == null)
            {
                List<OdinMenuItem> items;
                if (cachedAllTypesMenuItems.TryGetValue(this.assemblyTypeFlags, out items))
                {
                    AddRecursive(tree, items, tree.MenuItems);
                }
                else
                {
                    var assemblyTypes = OrderTypes(AssemblyUtilities.GetTypes(this.assemblyTypeFlags).Where(x => char.IsLetter(x.Name.Trim()[0])));
                    foreach (var t in assemblyTypes)
                    {
                        string path = string.IsNullOrEmpty(t.Namespace) ? t.GetNiceName() : t.Namespace + "/" + t.GetNiceName();
                        tree.AddObjectAtPath(path, t).AddThumbnailIcons();
                    }

                    cachedAllTypesMenuItems[this.assemblyTypeFlags] = tree.MenuItems;
                }
            }
            else
            {
                foreach (var t in this.types)
                {
                    string path = string.IsNullOrEmpty(t.Namespace) ? t.GetNiceName() : t.Namespace + "/" + t.GetNiceName();
                    tree.AddObjectAtPath(path, t);
                }

                tree.EnumerateTree(x => x.ObjectInstance != null, false).AddThumbnailIcons();

            }

            tree.EnumerateTree().ForEach(i =>
            {
                var t = i.ObjectInstance as Type;
                if (t != null) { i.SearchString = t.GetNiceFullName(); }
            });

            tree.Selection.SupportsMultiSelect = this.supportsMultiSelect;
            tree.Selection.SelectionChanged += (t) =>
            {
                lastType = this.SelectionTree.Selection.Select(x => x.ObjectInstance).OfType<Type>().LastOrDefault() ?? lastType;
            };
        }

        private static void AddRecursive(OdinMenuTree tree, List<OdinMenuItem> source, List<OdinMenuItem> destination)
        {
            destination.Capacity = source.Count;

            for (int i = 0; i < source.Count; i++)
            {
                var item = source[i];
                var clone = new OdinMenuItem(tree, item.Name, item.ObjectInstance)
                    .AddThumbnailIcon(false);

                destination.Add(clone);

                if (item.ChildMenuItems.Count > 0)
                {
                    AddRecursive(tree, item.ChildMenuItems, clone.ChildMenuItems);
                }
            }
        }

        private Type lastType;

        private struct Info
        {
            public string Label;
            public string Value;
            public Action OnClick;
        }

        /// <summary>
        /// 450
        /// </summary>
        protected override float DefaultWindowWidth()
        {
            return 450;
        }

        [OnInspectorGUI, PropertyOrder(10)]
        private void ShowTypeInfo()
        {
            var fullTypeName = "";
            var assembly = "";
            var baseType = "";
            var labelHeight = 16;
            var rect = GUILayoutUtility.GetRect(0, labelHeight * 3 + 20).Padding(10).AlignTop(labelHeight);
            var labelWidth = 75;

            if (lastType != null)
            {
                fullTypeName = lastType.GetNiceFullName();
                assembly = lastType.Assembly.GetName().Name;
                baseType = lastType.BaseType == null ? "" : lastType.BaseType.GetNiceFullName();
            }

            var style = SirenixGUIStyles.LeftAlignedGreyMiniLabel;
            GUI.Label(rect.AlignLeft(labelWidth), "Type Name", style);
            GUI.Label(rect.AlignRight(rect.width - labelWidth), fullTypeName, style);
            rect.y += labelHeight;
            GUI.Label(rect.AlignLeft(labelWidth), "Base Type", style);
            GUI.Label(rect.AlignRight(rect.width - labelWidth), baseType, style);
            rect.y += labelHeight;
            GUI.Label(rect.AlignLeft(labelWidth), "Assembly", style);
            GUI.Label(rect.AlignRight(rect.width - labelWidth), assembly, style);
        }

        /// <summary>
        /// Sets the selected types.
        /// </summary>
        public override void SetSelection(Type selected)
        {
            base.SetSelection(selected);

            // Expand so selected is visisble.
            this.SelectionTree.Selection.SelectMany(x => x.GetParentMenuItemsRecursive(false))
                .ForEach(x => x.Toggled = true);
        }

        //internal static Type DrawTypeField(GUIContent label, AssemblyTypeFlags flags, Type value)
        //{
        //    int id;
        //    bool hasFocus;
        //    Rect rect;
        //    Action<TypeSelector> bindSelector;
        //    Func<IEnumerable<Type>> resultGetter;

        //    var display = value == null ? "" : (EditorGUI.showMixedValue ? "—" : value.ToString());
        //    SirenixEditorGUI.GetFeatureRichControlRect(label, out id, out hasFocus, out rect);

        //    if (DrawSelectorButton(rect, display, EditorStyles.popup, id, out bindSelector, out resultGetter))
        //    {
        //        var selector = new TypeSelector(flags, false);
        //        if (!EditorGUI.showMixedValue)
        //        {
        //            selector.SetSelection(value);
        //        }

        //        selector.ShowInPopup(new Vector2(rect.xMin, rect.yMax));
        //        bindSelector(selector);
        //    }

        //    if (resultGetter != null)
        //    {
        //        value = resultGetter().FirstOrDefault();
        //        GUI.changed = true;
        //    }

        //    return value;
        //}
    }
}
#endif