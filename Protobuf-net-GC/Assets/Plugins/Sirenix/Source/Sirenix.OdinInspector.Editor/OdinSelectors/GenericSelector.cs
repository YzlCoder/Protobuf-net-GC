#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GenericSelector.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// In simple one-off use cases, making a custom OdinSelector might not be needed, as the GenericSelecor 
    /// can be populated with anything and customized a great deal.
    /// </summary>
    /// <example>
    /// <code>
    /// SomeType someValue;
    /// 
    /// [OnInspectorGUI]
    /// void OnInspectorGUI()
    /// {
    ///     if (GUILayout.Button("Open Generic Selector Popup"))
    ///     {
    ///         List&lt;SomeType&gt; source = ...;
    ///         GenericSelector&lt;SomeType&gt; selector = new GenericSelector&lt;SomeType&gt;("Title", false, x => x.Path, source);
    ///         selector.SetSelection(this.someValue);
    ///         selector.SelectionTree.Config.DrawSearchToolbar = false;
    ///         selector.SelectionTree.DefaultMenuStyle.Height = 22;
    ///         selector.SelectionConfirmed += selection =&gt; this.someValue = selection.FirstOrDefault()
    ///         var window = selector.ShowInPopup();
    ///         window.OnEndGUI += () =&gt; { EditorGUILayout.HelpBox("A quick way of injecting custom GUI to the editor window popup instance.", MessageType.Info); };
    ///         window.OnClose += selector.SelectionTree.Selection.ConfirmSelection; // Confirm selection when window clses.
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="OdinSelector{T}"/>
    /// <seealso cref="EnumSelector{T}"/>
    /// <seealso cref="TypeSelector"/>
    /// <seealso cref="OdinMenuTree"/>
    /// <seealso cref="OdinEditorWindow"/>
    public class GenericSelector<T> : OdinSelector<T>
    {
        private readonly string title;
        private readonly bool supportsMultiSelect;
        private readonly IEnumerable<T> collection;
        private Func<T, string> getMenuItemName;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelector{T}"/> class.
        /// </summary>
        public GenericSelector(string title, IEnumerable<T> collection, bool supportsMultiSelect, Func<T, string> getMenuItemName = null)
        {
            this.title = title;
            this.supportsMultiSelect = supportsMultiSelect;
            this.getMenuItemName = getMenuItemName;
            this.collection = collection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelector{T}"/> class.
        /// </summary>
        public GenericSelector(string title, bool supportsMultiSelect, Func<T, string> getMenuItemName, params T[] collection)
        {
            this.title = title;
            this.supportsMultiSelect = supportsMultiSelect;
            this.getMenuItemName = getMenuItemName ?? (x => x.ToString());
            this.collection = collection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelector{T}"/> class.
        /// </summary>
        public GenericSelector(string title, bool supportsMultiSelect, params T[] collection)
            : this(title, supportsMultiSelect, null, collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelector{T}"/> class.
        /// </summary>
        public GenericSelector(string title, params T[] collection)
            : this(title, false, null, collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelector{T}"/> class.
        /// </summary>
        public GenericSelector(params T[] collection)
            : this(null, false, null, collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelector{T}"/> class.
        /// </summary>
        public GenericSelector(string title, bool supportsMultiSelect, Func<T, string> getMenuItemName, IEnumerable<T> collection)
        {
            this.title = title;
            this.supportsMultiSelect = supportsMultiSelect;
            this.getMenuItemName = getMenuItemName ?? (x => x.ToString());
            this.collection = collection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelector{T}"/> class.
        /// </summary>
        public GenericSelector(string title, bool supportsMultiSelect, IEnumerable<T> collection)
            : this(title, supportsMultiSelect, null, collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelector{T}"/> class.
        /// </summary>
        public GenericSelector(string title, IEnumerable<T> collection)
            : this(title, false, null, collection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSelector{T}"/> class.
        /// </summary>
        public GenericSelector(IEnumerable<T> collection)
            : this(null, false, null, collection)
        {
        }

        /// <summary>
        /// Gets the title. No title will be drawn if the string is null or empty.
        /// </summary>
        public override string Title { get { return this.title; } }

        /// <summary>
        /// Builds the selection tree.
        /// </summary>
        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            tree.Selection.SupportsMultiSelect = this.supportsMultiSelect;
            this.getMenuItemName = this.getMenuItemName ?? (x => x == null ? "" : x.ToString());
            tree.AddRange(this.collection, this.getMenuItemName);
        }
    }


}
#endif