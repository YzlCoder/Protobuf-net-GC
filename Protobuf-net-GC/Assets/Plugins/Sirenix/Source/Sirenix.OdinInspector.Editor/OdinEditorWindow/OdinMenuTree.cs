#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinMenuTree.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;
    using Sirenix.Utilities;
    using UnityEngine;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEditor;
    using Sirenix.Utilities.Editor;
    using System.Text.RegularExpressions;
    using System.Collections;

    /// <summary>
    /// OdinMenuTree provides a tree of <see cref="OdinMenuItem"/>s, and helps with selection, inserting menu items into the tree, and can handle keyboard navigation for you.
    /// </summary>
    /// <example>
    /// <code>
    /// OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: true)
    /// {
    ///     { "Home",                           this,                           EditorIcons.House       },
    ///     { "Odin Settings",                  null,                           EditorIcons.SettingsCog },
    ///     { "Odin Settings/Color Palettes",   ColorPaletteManager.Instance,   EditorIcons.EyeDropper  },
    ///     { "Odin Settings/AOT Generation",   AOTGenerationConfig.Instance,   EditorIcons.SmartPhone  },
    ///     { "Camera current",                 Camera.current                                          },
    ///     { "Some Class",                     this.someData                                           }
    /// };
    /// 
    /// tree.AddAllAssetsAtPath("Some Menu Item", "Some Asset Path", typeof(ScriptableObject), true)
    ///     .AddThumbnailIcons();
    /// 
    /// tree.AddAssetAtPath("Some Second Menu Item", "SomeAssetPath/SomeAssetFile.asset");
    /// 
    /// var customMenuItem = new OdinMenuItem(tree, "Menu Style", tree.DefaultMenuStyle);
    /// tree.MenuItems.Insert(2, customMenuItem);
    /// 
    /// tree.Add("Menu/Items/Are/Created/As/Needed", new GUIContent());
    /// tree.Add("Menu/Items/Are/Created", new GUIContent("And can be overridden"));
    /// </code>
    /// OdinMenuTrees are typically used with <see cref="OdinMenuEditorWindow"/>s but is made to work perfectly fine on its own for other use cases.
    /// OdinMenuItems can be inherited and and customized to fit your needs.
    /// <code>
    /// // Draw stuff
    /// someTree.DrawMenuTree();
    /// // Draw stuff
    /// someTree.HandleKeybaordMenuNavigation();
    /// </code>
    /// </example>
    /// <seealso cref="OdinMenuItem" />
    /// <seealso cref="OdinMenuStyle" />
    /// <seealso cref="OdinMenuTreeSelection" />
    /// <seealso cref="OdinMenuTreeExtensions" />
    /// <seealso cref="OdinMenuEditorWindow" />
    public class OdinMenuTree : IEnumerable
    {
        private static HashSet<OdinMenuItem> cachedHashList = new HashSet<OdinMenuItem>();

        private readonly OdinMenuItem root;
        private readonly OdinMenuTreeSelection selection;
        private OdinMenuTreeDrawingConfig defaultConfig;
        private bool regainFocus;
        private Rect outerScrollViewRect;
        private float innerScrollViewYTop;
        private bool isFirstFrame = true;
        private int drawCount = 0;
        private bool requestRepaint;
        private bool updateScrollView;
        private GUIFrameCounter frameCounter = new GUIFrameCounter();
        private bool hasRepaintedCurrentSearchResult = true;
        private OdinMenuItem scrollToAndCenter;

        internal OdinMenuItem Root
        {
            get { return this.root; }
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        public OdinMenuTreeSelection Selection
        {
            get { return this.selection; }
        }

        /// <summary>
        /// Gets the root menu items.
        /// </summary>
        /// <value>
        /// The menu items.
        /// </value>
        public List<OdinMenuItem> MenuItems
        {
            get { return this.root.ChildMenuItems; }
        }

        /// <summary>
        /// If true, all indent levels will be ignored, and all menu items with IsVisible == true will be drawn.
        /// </summary>
        public bool DrawInSearchMode { get; set; }

        /// <summary>
        /// Adds a menu item with the specified object instance at the the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="instance">The instance.</param>
        public void Add(string path, object instance)
        {
            this.AddObjectAtPath(path, instance);
        }

        /// <summary>
        /// Adds a menu item with the specified object instance and icon at the the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="instance">The object instance.</param>
        /// <param name="icon">The icon.</param>
        public void Add(string path, object instance, Texture icon)
        {
            this.AddObjectAtPath(path, instance).AddIcon(icon);
        }

        /// <summary>
        /// Adds a menu item with the specified object instance and icon at the the specified path.
        /// </summary>
        /// <param name="path">The menu item path.</param>
        /// <param name="instance">The object instance.</param>
        /// <param name="icon">The icon.</param>
        public void Add(string path, object instance, EditorIcon icon)
        {
            this.AddObjectAtPath(path, instance).AddIcon(icon);
        }

        /// <summary>
        /// Adds a collection of objects to the menu tree and returns all menu items created in random order.
        /// </summary>
        public IEnumerable<OdinMenuItem> AddRange<T>(IEnumerable<T> collection, Func<T, string> getPath)
        {
            if (collection == null)
            {
                return Enumerable.Empty<OdinMenuItem>();
            }

            cachedHashList.Clear();

            foreach (var item in collection)
            {
                cachedHashList.AddRange(this.AddObjectAtPath(getPath(item), item));
            }

            return cachedHashList;
        }

        /// <summary>
        /// Adds a collection of objects to the menu tree and returns all menu items created in random order.
        /// </summary>
        public IEnumerable<OdinMenuItem> AddRange<T>(IEnumerable<T> collection, Func<T, string> getPath, Func<T, Texture> getIcon)
        {
            if (collection == null)
            {
                return Enumerable.Empty<OdinMenuItem>();
            }

            cachedHashList.Clear();

            foreach (var item in collection)
            {
                if (getIcon != null)
                {
                    cachedHashList.AddRange(this.AddObjectAtPath(getPath(item), item).AddIcon(getIcon(item)));
                }
                else
                {
                    cachedHashList.AddRange(this.AddObjectAtPath(getPath(item), item));
                }
            }

            return cachedHashList;
        }

        /// <summary>
        /// Gets or sets the default menu item style from Config.DefaultStyle.
        /// </summary>
        public OdinMenuStyle DefaultMenuStyle
        {
            get { return this.Config.DefaultMenuStyle; }
            set { this.Config.DefaultMenuStyle = value; }
        }

        /// <summary>
        /// Gets or sets the default drawing configuration.
        /// </summary>
        public OdinMenuTreeDrawingConfig Config
        {
            get
            {
                this.defaultConfig = defaultConfig ?? new OdinMenuTreeDrawingConfig()
                {
                    DrawScrollView = true,
                    DrawSearchToolbar = false,
                    AutoHandleKeyboardNavigation = false
                };

                return this.defaultConfig;
            }
            set
            {
                this.defaultConfig = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OdinMenuTree"/> class.
        /// </summary>
        /// <param name="supportsMultiSelect">if set to <c>true</c> [supports multi select].</param>
        public OdinMenuTree(bool supportsMultiSelect)
            : this(supportsMultiSelect, new OdinMenuStyle())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OdinMenuTree"/> class.
        /// </summary>
        /// <param name="supportsMultiSelect">if set to <c>true</c> [supports multi select].</param>
        /// <param name="defaultMenuStyle">The default menu item style.</param>
        public OdinMenuTree(bool supportsMultiSelect, OdinMenuStyle defaultMenuStyle)
        {
            this.DefaultMenuStyle = defaultMenuStyle;
            this.selection = new OdinMenuTreeSelection(supportsMultiSelect);
            this.root = new OdinMenuItem(this, "root", null);
            this.SetupAutoScroll();
        }

        private void SetupAutoScroll()
        {
            this.selection.SelectionChanged += (x) =>
            {
                if (this.Config.AutoScrollOnSelectionChanged)
                {
                    if (this.isFirstFrame)
                    {
                        this.scrollToAndCenter = this.selection.LastOrDefault();
                    }
                    else
                    {
                        this.requestRepaint = true;
                        GUIHelper.RequestRepaint();
                        ScrollToMenuItem(this.selection.LastOrDefault());
                    }
                }
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OdinMenuTree"/> class.
        /// </summary>
        public OdinMenuTree(bool supportsMultiSelect, OdinMenuTreeDrawingConfig config)
        {
            this.Config = config;
            this.selection = new OdinMenuTreeSelection(supportsMultiSelect);
            this.root = new OdinMenuItem(this, "root", null);
            this.SetupAutoScroll();
        }

        /// <summary>
        /// Scrolls to the specified menu item.
        /// </summary>
        public void ScrollToMenuItem(OdinMenuItem menuItem, bool centerMenuItem = false)
        {
            if (menuItem != null)
            {
                var config = this.Config;
                var rect = menuItem.Rect;

                float a, b;

                if (centerMenuItem)
                {
                    var r = this.outerScrollViewRect.AlignCenterY(rect.height);

                    a = rect.yMin - (this.innerScrollViewYTop + config.ScrollPos.y - r.y);
                    b = (rect.yMax - r.height + this.innerScrollViewYTop) - (config.ScrollPos.y + r.y);
                }
                else
                {
                    a = rect.yMin - (this.innerScrollViewYTop + config.ScrollPos.y - this.outerScrollViewRect.y);
                    b = (rect.yMax - this.outerScrollViewRect.height + this.innerScrollViewYTop) - (config.ScrollPos.y + this.outerScrollViewRect.y);
                }


                if (a < 0)
                {
                    config.ScrollPos.y += a;
                }

                if (b > 0)
                {
                    config.ScrollPos.y += b;
                }
            }
        }

        /// <summary>
        /// Enumerates the tree with a DFS.
        /// </summary>
        /// <param name="includeRootNode">if set to <c>true</c> then the invisible root menu item is included.</param>
        public IEnumerable<OdinMenuItem> EnumerateTree(bool includeRootNode = false)
        {
            return this.root.GetChildMenuItemsRecursive(includeRootNode);
        }

        /// <summary>
        /// Enumerates the tree with a DFS.
        /// </summary>
        /// <param name="includeRootNode">if set to <c>true</c> then the invisible root menu item is included.</param>
        public IEnumerable<OdinMenuItem> EnumerateTree(Func<OdinMenuItem, bool> predicate, bool includeRootNode)
        {
            return this.root.GetChildMenuItemsRecursive(includeRootNode).Where(predicate);
        }

        /// <summary>
        /// Enumerates the tree with a DFS.
        /// </summary>
        public void EnumerateTree(Action<OdinMenuItem> action)
        {
            this.root.GetChildMenuItemsRecursive(false).ForEach(action);
        }

        /// <summary>
        /// Draws the menu tree recursively.
        /// </summary>
        public void DrawMenuTree()
        {
            var config = this.Config;

            if (this.requestRepaint)
            {
                GUIHelper.RequestRepaint();
                this.requestRepaint = false;
            }

            if (config.DrawSearchToolbar)
            {
                DrawSearchToolbar();
            }

            if (config.DrawScrollView)
            {
                var r = EditorGUILayout.BeginVertical();
                if (Event.current.type == EventType.Repaint)
                {
                    this.outerScrollViewRect = r;
                }

                // GUIScrollViews doesn't play well with ExpandHeight(false). The scroll wheel will flicker when it expanding or contracting in size.
                // But for popup windows and other stuff, it is important that the ExpandHeight is false during the first couple of frames
                // so we can calculate a good height for the window.

                if (frameCounter.Update().FrameCount < 4)
                {
                    config.ScrollPos = EditorGUILayout.BeginScrollView(config.ScrollPos, GUILayoutOptions.ExpandHeight(false));
                }
                else
                {
                    config.ScrollPos = EditorGUILayout.BeginScrollView(config.ScrollPos);
                }

                if (Event.current.type == EventType.Repaint)
                {
                    this.innerScrollViewYTop = GUIHelper.GetCurrentLayoutRect().y;
                }

                GUILayout.Space(-1);
            }

            foreach (var item in this.MenuItems)
            {
                item.DrawMenuItems(0);
            }

            if (config.DrawScrollView)
            {
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();

                if (!this.isFirstFrame)
                {
                    if (this.scrollToAndCenter != null && Event.current.type == EventType.Layout)
                    {
                        this.ScrollToMenuItem(this.Selection.LastOrDefault(), true);
                        this.scrollToAndCenter = null;
                    }

                    if (this.updateScrollView)
                    {
                        this.ScrollToMenuItem(this.Selection.LastOrDefault());
                        this.updateScrollView = false;
                    }
                }
            }

            if (config.AutoHandleKeyboardNavigation)
            {
                this.HandleKeybaordMenuNavigation();
            }

            if (Event.current.type == EventType.Repaint)
            {
                this.isFirstFrame = false;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        /// <param name="toolbarStyle">Not yet documented.</param>
        public void DrawSearchToolbar(GUIStyle toolbarStyle = null)
        {
            var config = this.Config;

            var searchFieldRect = GUILayoutUtility.GetRect(0, config.SearchToolbarHeight, GUILayoutOptions.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
            {
                (toolbarStyle ?? SirenixGUIStyles.ToolbarBackground).Draw(searchFieldRect, GUIContent.none, 0);
            }

            searchFieldRect = searchFieldRect.HorizontalPadding(5).AlignMiddle(16);
            searchFieldRect.xMin += 3;
            searchFieldRect.y += 1;

            EditorGUI.BeginChangeCheck();
            config.SearchTerm = this.DrawSearchField(searchFieldRect, config.SearchTerm, config.AutoFocusSearchBar);
            var changed = EditorGUI.EndChangeCheck();

            if (changed && this.hasRepaintedCurrentSearchResult)
            {
                // We want fast visual search feedback. If the user is typing faster than the window can repaint,
                // then no results will be visible while he's typing. this.hasRepaintedCurrentSearchResult fixes that.

                this.hasRepaintedCurrentSearchResult = false;
                bool doSearch = !string.IsNullOrEmpty(config.SearchTerm);
                if (doSearch)
                {
                    this.DrawInSearchMode = true;
                    this.EnumerateTree().ForEach(x => x.IsVisible = config.SearchFunction(x));
                }
                else
                {
                    if (this.DrawInSearchMode)
                    {
                        this.DrawInSearchMode = false;
                        var last = this.selection.LastOrDefault();
                        UnityEditorEventUtility.DelayAction(() => this.scrollToAndCenter = last);
                    }

                    this.EnumerateTree()
                        .ForEach(x => x.IsVisible = true);

                    this.Selection
                        .SelectMany(x => x.GetParentMenuItemsRecursive(false))
                        .ForEach(x => x.Toggled = true);
                }
            }

            if (Event.current.type == EventType.Repaint)
            {
                this.hasRepaintedCurrentSearchResult = true;
            }
        }

        private string DrawSearchField(Rect rect, string searchTerm, bool autoFocus)
        {
            bool ignore =
                GUI.GetNameOfFocusedControl() == "SirenixSearchField" &&
               (Event.current.keyCode == KeyCode.DownArrow ||
                Event.current.keyCode == KeyCode.UpArrow ||
                Event.current.keyCode == KeyCode.LeftArrow ||
                Event.current.keyCode == KeyCode.RightArrow ||
                Event.current.keyCode == KeyCode.Return);

            if (ignore)
            {
                GUIHelper.PushEventType(Event.current.type);
            }

            searchTerm = SirenixEditorGUI.SearchField(rect, searchTerm, autoFocus && this.regainFocus, "SirenixSearchField");

            if (this.regainFocus && Event.current.type == EventType.Layout)
            {
                this.regainFocus = false;
            }

            if (ignore)
            {
                GUIHelper.PopEventType();
                this.regainFocus = true;
            }

            if (this.drawCount < 20)
            {
                if (autoFocus && this.drawCount < 4)
                {
                    this.regainFocus = true;
                }

                GUIHelper.RequestRepaint();
                HandleUtility.Repaint();
                if (Event.current.type == EventType.Repaint)
                {
                    this.drawCount++;
                }
            }

            return searchTerm;
        }

        /// <summary>
        /// Updates the menu tree. This method is usually called automatically when needed.
        /// </summary>
        public void UpdateMenuTree()
        {
            this.root.UpdateMenuTreeRecursive(true);
        }

        /// <summary>
        /// Handles the keybaord menu navigation. Call this at the end of your GUI scope, to prevent the menu tree from stealing input events from text fields and such.
        /// </summary>
        /// <returns>Returns true, if anything was changed via the keyboard.</returns>
        public bool HandleKeybaordMenuNavigation()
        {
            if (Event.current.type != EventType.KeyDown)
            {
                return false;
            }

            GUIHelper.RequestRepaint();

            var keycode = Event.current.keyCode;

            // Select first or last if no visisble items is slected.
            if (this.Selection.Count == 0 || !this.Selection.Any(x => x.IsVisibleRecrusive()))
            {
                OdinMenuItem next = null;
                if (keycode == KeyCode.DownArrow)
                {
                    next = this.EnumerateTree().Where(x => x.IsVisibleRecrusive()).FirstOrDefault();
                }
                else if (keycode == KeyCode.UpArrow)
                {
                    next = this.EnumerateTree().Where(x => x.IsVisibleRecrusive()).LastOrDefault();
                }
                else if (keycode == KeyCode.LeftAlt)
                {
                    next = this.EnumerateTree().Where(x => x.IsVisibleRecrusive()).FirstOrDefault();
                }
                else if (keycode == KeyCode.RightAlt)
                {
                    next = this.EnumerateTree().Where(x => x.IsVisibleRecrusive()).FirstOrDefault();
                }

                if (next != null)
                {
                    next.Select();
                    this.updateScrollView = true;
                    Event.current.Use();
                    return true;
                }
            }
            else
            {
                if (keycode == KeyCode.LeftArrow && !this.DrawInSearchMode)
                {
                    bool goUp = true;
                    foreach (var curr in this.Selection.ToList())
                    {
                        if (curr.Toggled == true && curr.ChildMenuItems.Any())
                        {
                            goUp = false;
                            curr.Toggled = false;
                        }

                        if ((Event.current.modifiers & EventModifiers.Alt) != 0)
                        {
                            goUp = false;
                            foreach (var item in curr.GetChildMenuItemsRecursive(false))
                            {
                                item.Toggled = curr.Toggled;
                            }
                        }
                    }

                    if (goUp)
                    {
                        keycode = KeyCode.UpArrow;
                    }

                    Event.current.Use();
                }

                if (keycode == KeyCode.RightArrow && !this.DrawInSearchMode)
                {
                    bool goDown = true;
                    foreach (var curr in this.Selection.ToList())
                    {
                        if (curr.Toggled == false && curr.ChildMenuItems.Any())
                        {
                            curr.Toggled = true;
                            goDown = false;
                        }

                        if ((Event.current.modifiers & EventModifiers.Alt) != 0)
                        {
                            goDown = false;

                            foreach (var item in curr.GetChildMenuItemsRecursive(false))
                            {
                                item.Toggled = curr.Toggled;
                            }
                        }
                    }

                    if (goDown)
                    {
                        keycode = KeyCode.DownArrow;
                    }

                    Event.current.Use();
                }

                if (keycode == KeyCode.UpArrow)
                {
                    if ((Event.current.modifiers & EventModifiers.Shift) != 0)
                    {
                        var last = this.Selection.Last();
                        var prev = last.PrevVisualMenuItem;

                        if (prev != null)
                        {
                            if (prev.IsSelected)
                            {
                                last.Deselect();
                            }
                            else
                            {
                                prev.Select(true);
                            }

                            this.updateScrollView = true;
                            Event.current.Use();
                            return true;
                        }
                    }
                    else
                    {
                        var prev = this.Selection.Last().PrevVisualMenuItem;
                        if (prev != null)
                        {
                            prev.Select();
                            this.updateScrollView = true;
                            Event.current.Use();
                            return true;
                        }
                    }
                }

                if (keycode == KeyCode.DownArrow)
                {
                    if ((Event.current.modifiers & EventModifiers.Shift) != 0)
                    {
                        var last = this.Selection.Last();
                        var next = last.NextVisualMenuItem;

                        if (next != null)
                        {
                            if (next.IsSelected)
                            {
                                last.Deselect();
                            }
                            else
                            {
                                next.Select(true);
                            }

                            this.updateScrollView = true;
                            Event.current.Use();
                            return true;
                        }
                    }
                    else
                    {
                        var next = this.Selection.Last().NextVisualMenuItem;
                        if (next != null)
                        {
                            next.Select();
                            this.updateScrollView = true;
                            Event.current.Use();
                            return true;
                        }
                    }
                }

                if (keycode == KeyCode.Return)
                {
                    this.Selection.ConfirmSelection();
                    Event.current.Use();
                }
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.MenuItems.GetEnumerator();
        }
    }
}
#endif