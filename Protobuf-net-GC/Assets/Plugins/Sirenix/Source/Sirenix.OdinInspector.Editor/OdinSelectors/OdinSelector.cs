#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinSelector.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using Sirenix.Utilities;
    using UnityEditor;
    using UnityEngine;
    using Sirenix.Utilities.Editor;
    using System.Linq;

    /// <summary>
    /// OdinSelectors is an abstract base class that combines OdinMenuTrees and OdinEditorWindows to help making feature-rich selectors and popup selectors.
    /// </summary>
    /// <example>
    /// <code>
    /// public class MySelector : OdinSelector&lt;SomeType&gt;
    /// {
    ///     private readonly List&lt;SomeType&gt; source;
    ///     private readonly bool supportsMultiSelect;
    /// 
    ///     public MySelector(List&lt;SomeType&gt; source, bool supportsMultiSelect)
    ///     {
    ///         this.source = source;
    ///         this.supportsMultiSelect = supportsMultiSelect;
    ///     }
    /// 
    ///     protected override void BuildSelectionTree(OdinMenuTree tree)
    ///     {
    ///         tree.Config.DrawSearchToolbar = true;
    ///         tree.Selection.SupportsMultiSelect = this.supportsMultiSelect;
    ///         
    ///         tree.Add("Defaults/None", null);
    ///         tree.Add("Defaults/A", new SomeType());
    ///         tree.Add("Defaults/B", new SomeType());
    /// 
    ///         tree.AddRange(this.source, x =&gt; x.Path, x =&gt; x.SomeTexture);
    ///     }
    /// 
    ///     [OnInspectorGUI]
    ///     private void DrawInfoAboutSelectedItem()
    ///     {
    ///         SomeType selected = this.GetCurrentSelection().FirstOrDefault();
    /// 
    ///         if (selected != null)
    ///         {
    ///             GUILayout.Label("Name: " + selected.Name);
    ///             GUILayout.Label("Data: " + selected.Data);
    ///         }
    ///     }
    /// }
    /// </code>
    /// Usage:
    /// <code>
    /// void OnGUI()
    /// {
    ///     if (GUILayout.Button("Open My Selector"))
    ///     {
    ///         List&lt;SomeType&gt; source = this.GetListOfThingsToSelectFrom();
    ///         MySelector selector = new MySelector(source, false);
    /// 
    ///         selector.SetSelection(this.someValue);
    /// 
    ///         selector.SelectionCancelled += () =&gt; { };  // Occurs when the popup window is closed, and no slection was confirmed.
    ///         selector.SelectionChanged += col =&gt; { };
    ///         selector.SelectionConfirmed += col =&gt; this.someValue = col.FirstOrDefault();
    /// 
    ///         selector.ShowInPopup(); // Returns the Odin Editor Window instance, in case you want to mess around with that as well.
    ///     }
    /// }
    /// 
    /// // All Odin Selectors can be rendered anywhere with Odin.
    /// [ShowInInspector]
    /// MySelector inlineSelector;
    /// </code>
    /// </example>
    /// <seealso cref="EnumSelector{T}"/>
    /// <seealso cref="TypeSelector"/>
    /// <seealso cref="GenericSelector{T}"/>
    /// <seealso cref="OdinMenuTree"/>
    /// <seealso cref="OdinEditorWindow"/>
    public abstract class OdinSelector<T>
    {
        private static EditorWindow selectorFieldWindow = null;
        private static IEnumerable<T> selectedValues = null;
        private static bool selectionWasConfirmed = false;
        private static int confirmedPopupControlId = -1;
        private static int focusedControlId = -1;
        private static GUIStyle titleStyle = null;

        private OdinEditorWindow popupWindowInstance;
        private OdinMenuTree selectionTree;

        [HideInInspector]
        public bool DrawConfirmSelectionButton = false;

        [SerializeField, HideInInspector]
        private OdinMenuTreeDrawingConfig config = new OdinMenuTreeDrawingConfig()
        {
            SearchToolbarHeight = 22,
            AutoScrollOnSelectionChanged = true,
            DefaultMenuStyle = new OdinMenuStyle()
            {
                Height = 22
            }
        };

        private static bool wasKeyboard;

        private static int prevKeybaordId;

        /// <summary>
        /// Occurs when the window is closed, and no slection was confirmed.
        /// </summary>
        public event Action SelectionCancelled;

        /// <summary>
        /// Occurs when the menuTrees selection is changed and IsValidSelection returns true.
        /// </summary>
        public event Action<IEnumerable<T>> SelectionChanged;

        /// <summary>
        /// Occurs when the menuTrees selection is confirmed and IsValidSelection returns true.
        /// </summary>
        public event Action<IEnumerable<T>> SelectionConfirmed;

        /// <summary>
        /// Gets the selection menu tree.
        /// </summary>
        public OdinMenuTree SelectionTree
        {
            get
            {
                if (this.selectionTree == null)
                {
                    this.selectionTree = new OdinMenuTree(true);
                    this.selectionTree.Config = this.config;

                    this.BuildSelectionTree(this.selectionTree);

                    this.selectionTree.Selection.SelectionConfirmed += x =>
                    {
                        if (this.SelectionConfirmed != null)
                        {
                            IEnumerable<T> selected = this.GetCurrentSelection();
                            if (this.IsValidSelection(selected))
                            {
                                this.SelectionConfirmed(selected);
                            }
                        }
                    };

                    this.selectionTree.Selection.SelectionChanged += x =>
                    {
                        TriggerSelectionChanged();
                    };
                }

                return this.selectionTree;
            }
        }

        /// <summary>
        /// Gets the title. No title will be drawn if the string is null or empty.
        /// </summary>
        public virtual string Title { get { return null; } }

        /// <summary>
        /// Gets the current selection from the menu tree whether it's valid or not.
        /// </summary>
        public virtual IEnumerable<T> GetCurrentSelection()
        {
            return this.SelectionTree.Selection
                .Select(x => x.ObjectInstance)
                .OfType<T>();
        }

        /// <summary>
        /// Determines whether the specified collection is a valid collection.
        /// If false, the SlectionChanged and SelectionConfirm events will not be called.
        /// By default, this returns true if the collection contains one or more items.
        /// </summary>
        public virtual bool IsValidSelection(IEnumerable<T> collection)
        {
            return collection.Any();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        public void SetSelection(IEnumerable<T> selection)
        {
            this.SelectionTree.Selection.Clear();

            if (selection == null) return;

            foreach (var item in selection)
            {
                this.SetSelection(item);
            }
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        public virtual void SetSelection(T selected)
        {
            if (selected == null) return;

            var items = this.SelectionTree.EnumerateTree()
                .Where(x => x.ObjectInstance is T)
                .Where(x => EqualityComparer<T>.Default.Equals((T)x.ObjectInstance, selected))
                .ToList();

            items.ForEach(x => x.Select(true));
        }

        /// <summary>
        /// Opens up the selector instance in a popup at the specified rect position.
        /// The width of the popup is determined by DefaultWindowWidth, and the height is automatically calculated.
        /// </summary>
        public OdinEditorWindow ShowInPopup()
        {
            var prevSelectedWindow = EditorWindow.focusedWindow;

            OdinEditorWindow window;

            var width = this.DefaultWindowWidth();
            if (width == 0)
            {
                window = OdinEditorWindow.InspectObjectInDropDown(this);
            }
            else
            {
                window = OdinEditorWindow.InspectObjectInDropDown(this, width);
            }

            this.SetupWindow(window, prevSelectedWindow);

            return window;
        }

        /// <summary>
        /// Opens up the selector instance in a popup at the specified rect position.
        /// </summary>
        public OdinEditorWindow ShowInPopup(Rect benRect, float windowWidth)
        {
            var prevSelectedWindow = EditorWindow.focusedWindow;
            OdinEditorWindow window = OdinEditorWindow.InspectObjectInDropDown(this, benRect, windowWidth);
            SetupWindow(window, prevSelectedWindow);
            return window;
        }

        /// <summary>
        /// The mouse position is used as the position for the window.
        /// Opens up the selector instance in a popup at the specified position.
        /// </summary>
        public OdinEditorWindow ShowInPopup(float windowWidth)
        {
            var prevSelectedWindow = EditorWindow.focusedWindow;
            OdinEditorWindow window = OdinEditorWindow.InspectObjectInDropDown(this, windowWidth);
            SetupWindow(window, prevSelectedWindow);
            return window;
        }

        /// <summary>
        /// Opens up the selector instance in a popup at the specified position.
        /// </summary>
        public OdinEditorWindow ShowInPopup(Vector2 position, float windowWidth)
        {
            var prevSelectedWindow = EditorWindow.focusedWindow;
            OdinEditorWindow window = OdinEditorWindow.InspectObjectInDropDown(this, position, windowWidth);
            SetupWindow(window, prevSelectedWindow);
            return window;
        }

        /// <summary>
        /// Opens up the selector instance in a popup at the specified rect position.
        /// </summary>
        public OdinEditorWindow ShowInPopup(Rect btnRect, Vector2 windowSize)
        {
            var prevSelectedWindow = EditorWindow.focusedWindow;
            OdinEditorWindow window = OdinEditorWindow.InspectObjectInDropDown(this, btnRect, windowSize);
            SetupWindow(window, prevSelectedWindow);
            return window;
        }

        /// <summary>
        /// Opens up the selector instance in a popup at the specified position.
        /// The width of the popup is determined by DefaultWindowWidth, and the height is automatically calculated.
        /// </summary>
        public OdinEditorWindow ShowInPopup(Vector2 position)
        {
            var prevSelectedWindow = EditorWindow.focusedWindow;
            OdinEditorWindow window;

            var width = this.DefaultWindowWidth();
            if (width == 0)
            {
                window = OdinEditorWindow.InspectObjectInDropDown(this, position);
            }
            else
            {
                window = OdinEditorWindow.InspectObjectInDropDown(this, position, width);
            }

            SetupWindow(window, prevSelectedWindow);
            return window;
        }

        /// <summary>
        /// Opens up the selector instance in a popup with the specified width and height.
        /// The mouse position is used as the position for the window.
        /// </summary>
        public OdinEditorWindow ShowInPopup(float width, float height)
        {
            var prevSelectedWindow = EditorWindow.focusedWindow;
            OdinEditorWindow window = OdinEditorWindow.InspectObjectInDropDown(this, width, height);
            SetupWindow(window, prevSelectedWindow);
            return window;
        }

        protected abstract void BuildSelectionTree(OdinMenuTree tree);

        /// <summary>
        /// When ShowInPopup is called, without a specifed window width, this methods gets called.
        /// Here you can calculate and give a good default width for the popup. 
        /// The default implementation returns 0, which will let the popup window determain the width itself. This is usually a fixed value.
        /// </summary>
        protected virtual float DefaultWindowWidth()
        {
            return 0;
        }

        /// <summary>
        /// Triggers the selection changed event, but only if the current selection is valid.
        /// </summary>
        protected void TriggerSelectionChanged()
        {
            if (this.SelectionChanged != null)
            {
                IEnumerable<T> selected = this.GetCurrentSelection();
                if (this.IsValidSelection(selected))
                {
                    this.SelectionChanged(selected);
                }
            }
        }

        /// <summary>
        /// Draws the selection tree. This gets drawn using the OnInspectorGUI attribute.
        /// </summary>
        [OnInspectorGUI]
        [PropertyOrder(-1)]
        protected virtual void DrawSelectionTree()
        {
            var rect = EditorGUILayout.BeginVertical();
            {
                EditorGUI.DrawRect(rect, SirenixGUIStyles.DarkEditorBackground);
                GUILayout.Space(1);

                bool drawTitle = !string.IsNullOrEmpty(this.Title);
                bool drawSearchToolbar = this.SelectionTree.Config.DrawSearchToolbar;
                bool drawButton = this.DrawConfirmSelectionButton;

                if (drawTitle || drawSearchToolbar || drawButton)
                {
                    SirenixEditorGUI.BeginHorizontalToolbar(this.SelectionTree.Config.SearchToolbarHeight);
                    {
                        if (drawTitle)
                        {
                            if (titleStyle == null)
                            {
                                titleStyle = new GUIStyle(SirenixGUIStyles.LeftAlignedCenteredLabel) { padding = new RectOffset(10, 10, 0, 0) };
                            }

                            var labelRect = GUILayoutUtility.GetRect(new GUIContent(this.Title), titleStyle, GUILayoutOptions.ExpandWidth(false).Height(this.SelectionTree.Config.SearchToolbarHeight));

                            if (Event.current.type == EventType.Repaint)
                            {
                                labelRect.y -= 2;
                                GUI.Label(labelRect.AlignCenterY(16), this.Title, titleStyle);
                            }
                        }

                        if (drawSearchToolbar)
                        {
                            this.SelectionTree.DrawSearchToolbar(GUIStyle.none);
                        }
                        else
                        {
                            GUILayout.FlexibleSpace();
                        }

                        EditorGUI.DrawRect(GUILayoutUtility.GetLastRect().AlignLeft(1), SirenixGUIStyles.BorderColor);

                        if (drawButton && SirenixEditorGUI.ToolbarButton(new GUIContent(EditorIcons.TestPassed)))
                        {
                            this.SelectionTree.Selection.ConfirmSelection();
                        }
                    }
                    SirenixEditorGUI.EndHorizontalToolbar();
                }

                var prev = this.SelectionTree.Config.DrawSearchToolbar;
                this.SelectionTree.Config.DrawSearchToolbar = false;
                this.SelectionTree.DrawMenuTree();
                this.SelectionTree.Config.DrawSearchToolbar = prev;

                SirenixEditorGUI.DrawBorders(rect, 1);
            }
            EditorGUILayout.EndVertical();
        }

        private void SetupWindow(OdinEditorWindow window, EditorWindow prevSelectedWindow)
        {
            var prevFocusId = GUIUtility.hotControl;
            var prevKeybaorFocus = GUIUtility.keyboardControl;
            this.popupWindowInstance = window;

            window.WindowPadding = new Vector4();

            bool wasConfirmed = false;

            this.SelectionTree.Selection.SelectionConfirmed += x => UnityEditorEventUtility.DelayAction(() =>
            {
                if (this.IsValidSelection(this.GetCurrentSelection()))
                {
                    wasConfirmed = true;
                    window.Close();

                    if (prevSelectedWindow)
                    {
                        prevSelectedWindow.Focus();
                    }
                }
            });

            window.OnBeginGUI += () =>
            {
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
                {
                    UnityEditorEventUtility.DelayAction(() =>
                    {
                        window.Close();
                    });

                    if (prevSelectedWindow)
                    {
                        prevSelectedWindow.Focus();
                    }

                    Event.current.Use();
                }
            };

            window.OnClose += () =>
            {
                if (!wasConfirmed && this.SelectionCancelled != null)
                {
                    this.SelectionCancelled();
                }

                GUIUtility.hotControl = prevFocusId;
                GUIUtility.keyboardControl = prevKeybaorFocus;
            };
        }

        internal static bool DrawSelectorButton<TSelector>(Rect buttonRect, string label, GUIStyle style, int id, out Action<TSelector> bindSelector, out Func<IEnumerable<T>> resultGetter)
            where TSelector : OdinSelector<T>
        {
            return DrawSelectorButton(buttonRect, new GUIContent(label), style, id, out bindSelector, out resultGetter);
        }

        internal static bool DrawSelectorButton<TSelector>(Rect buttonRect, GUIContent label, GUIStyle style, int id, out Action<TSelector> bindSelector, out Func<IEnumerable<T>> resultGetter)
            where TSelector : OdinSelector<T>
        {
            var wasPressed = false;
            bindSelector = null;
            resultGetter = null;

            if (Event.current.type == EventType.Repaint)
            {
                var showIsDown = GUIUtility.hotControl == id || focusedControlId == id;
                EditorStyles.popup.Draw(buttonRect, label, showIsDown, showIsDown, false, GUIUtility.keyboardControl == id);
            }

            bool openPopup = false;

            if (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown && GUIUtility.keyboardControl == id)
            {
                GUIUtility.hotControl = id;
                wasKeyboard = true;
            }
            else if (GUIUtility.hotControl == id && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyUp && GUIUtility.keyboardControl == id)
            {
                openPopup = true;
                wasKeyboard = true;
            }
            else if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && buttonRect.Contains(Event.current.mousePosition))
            {
                GUIUtility.hotControl = id;
                wasKeyboard = false;
            }
            else if (GUIUtility.hotControl == id && Event.current.type == EventType.MouseUp && Event.current.button == 0 && buttonRect.Contains(Event.current.mousePosition))
            {
                openPopup = true;
                wasKeyboard = false;
            }

            if (openPopup)
            {
                prevKeybaordId = GUIUtility.keyboardControl;
                selectedValues = null;
                selectionWasConfirmed = false;
                focusedControlId = id;
                selectorFieldWindow = EditorWindow.focusedWindow;

                GUIUtility.hotControl = id;

                if (wasKeyboard)
                {
                    GUIUtility.keyboardControl = id;
                }

                bindSelector = selector =>
                {
                    selector.SelectionChanged += x => selectedValues = x;
                    selector.SelectionConfirmed += x =>
                    {
                        selectionWasConfirmed = true;
                        selectedValues = x;
                        confirmedPopupControlId = id;
                    };

                    var window = selector.popupWindowInstance;
                    if (window != null)
                    {
                        window.OnClose += () => focusedControlId = -1;
                        window.OnClose += () => confirmedPopupControlId = id;
                    }
                };

                wasPressed = true;
                Event.current.Use();
            }

            if (Event.current.type == EventType.Repaint && selectorFieldWindow == GUIHelper.CurrentWindow && id == confirmedPopupControlId)
            {
                selectorFieldWindow = null;
                confirmedPopupControlId = -1;
                focusedControlId = -1;

                if (wasKeyboard)
                {
                    GUIUtility.keyboardControl = prevKeybaordId;
                }
                else
                {
                    GUIUtility.keyboardControl = -1;
                }

                if (selectionWasConfirmed)
                {
                    GUI.changed = true;

                    resultGetter = () => selectedValues ?? Enumerable.Empty<T>();
                }

                selectionWasConfirmed = false;
            }

            return wasPressed;
        }
    }
}
#endif