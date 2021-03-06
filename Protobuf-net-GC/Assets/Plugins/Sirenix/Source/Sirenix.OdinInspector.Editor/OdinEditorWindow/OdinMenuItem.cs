#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinMenuItem.cs" company="Sirenix IVS">
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
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using System.Collections;

    /// <summary>
    /// A menu item that represents one or more objects.
    /// </summary>
    /// <seealso cref="OdinMenuTree" />
    /// <seealso cref="OdinMenuStyle" />
    /// <seealso cref="OdinMenuTreeSelection" />
    /// <seealso cref="OdinMenuTreeExtensions" />
    /// <seealso cref="OdinMenuEditorWindow" />
    public class OdinMenuItem
    {
        private static Color mouseOverColor = EditorGUIUtility.isProSkin ? new Color(1.000f, 1.000f, 1.000f, 0.028f) : new Color(1.000f, 1.000f, 1.000f, 0.3f);
        private static OdinMenuItem handleClickEventOnMouseUp;
        private static bool previousMenuItemWasSelected = false;

        private List<OdinMenuItem> childMenuItems;
        private int flatTreeIndex;
        private Texture iconSelected;
        private Texture icon;
        private bool isInitialized = false;
        private LocalPersistentContext<bool> isToggledContext;
        private OdinMenuTree menuTree;
        private string prevName;
        private string name;
        private bool isVisible = true;
        private OdinMenuItem nextMenuItem;
        private OdinMenuItem nextMenuItemFlat;
        private IList objectInstances;
        private OdinMenuItem parentMenuItem;
        private OdinMenuItem previousMenuItem;
        private OdinMenuItem previousMenuItemFlat;
        private Rect rect;
        private OdinMenuStyle style;
        private Rect triangleRect;
        private Rect labelRect;
        private StringMemberHelper nameValueGetter;

        public bool DefaultToggledState = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="OdinMenuItem"/> class.
        /// </summary>
        /// <param name="tree">The Odin menu tree instance the menu item belongs to.</param>
        /// <param name="name">The name of the menu item.</param>
        /// <param name="instances">The instances the menu item represents.</param>
        public OdinMenuItem(OdinMenuTree tree, string name, IList instances)
        {
            if (tree == null) throw new ArgumentNullException("tree");
            if (name == null) throw new ArgumentNullException("name");

            this.menuTree = tree;
            this.name = name;
            this.SearchString = name;
            this.objectInstances = instances;
            this.childMenuItems = new List<OdinMenuItem>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OdinMenuItem"/> class.
        /// </summary>
        /// <param name="tree">The Odin menu tree instance the menu item belongs to.</param>
        /// <param name="name">The name of the menu item.</param>
        /// <param name="instance">The instance the menu item represents.</param>
        public OdinMenuItem(OdinMenuTree tree, string name, object instance)
            : this(tree, name, new object[] { instance })
        {
        }

        /// <summary>
        /// Occurs right after the menu item is done drawing, and right before mouse input is handles so you can take control of that.
        /// </summary>
        public event Action<OdinMenuItem> OnDrawItem;

        /// <summary>
        /// Occurs when the user has right-clicked the menu item.
        /// </summary>
        public event Action<OdinMenuItem> OnRightClick;

        /// <summary>
        /// Gets the child menu items.
        /// </summary>
        /// <value>
        /// The child menu items.
        /// </value>
        public virtual List<OdinMenuItem> ChildMenuItems
        {
            get { return this.childMenuItems; }
        }

        /// <summary>
        /// Gets the index location of the menu item.
        /// </summary>
        public int FlatTreeIndex
        {
            get { return this.flatTreeIndex; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the menu item is visible.
        /// Not that setting this to false will not hide its children as well. For that see use Toggled.
        /// </summary>
        public virtual bool IsVisible
        {
            get { return this.isVisible; }
            set { this.isVisible = value; }
        }

        /// <summary>
        /// Gets or sets the icon that is used when the menu item is not selected.
        /// </summary>
        public Texture Icon
        {
            get { return this.icon; }
            set { this.icon = value; }
        }

        /// <summary>
        /// Gets or sets the icon that is used when the menu item is selected.
        /// </summary>
        public Texture IconSelected
        {
            get { return this.iconSelected; }
            set { this.iconSelected = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return this.menuTree.Selection.Contains(this); }
        }

        /// <summary>
        /// Gets the menu tree instance.
        /// </summary>
        public OdinMenuTree MenuTree
        {
            get { return this.menuTree; }
        }

        /// <summary>
        /// Gets or sets the raw menu item name.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /// <summary>
        /// Gets or sets the search string used when searching for menu items.
        /// </summary>
        public string SearchString { get; set; }

        /// <summary>
        /// Gets the next visual menu item.
        /// </summary>
        public OdinMenuItem NextVisualMenuItem
        {
            get
            {
                this.EnsureInitialized();

                // Performance optimization:
                if (this.ChildMenuItems.Count > 0 && this.nextMenuItem != null && this.Toggled == false && this.nextMenuItem.IsVisible && this.IsVisibleRecrusive())
                {
                    return this.nextMenuItem;
                }

                // Bruteforce search:
                return this.GetAllNextMenuItems().FirstOrDefault(x => x.IsVisibleRecrusive());
            }
        }

        /// <summary>
        /// Gets the parent menu item.
        /// </summary>
        public OdinMenuItem Parent
        {
            get
            {
                this.EnsureInitialized();
                return this.parentMenuItem;
            }
        }

        /// <summary>
        /// Gets the previous visual menu item.
        /// </summary>
        public OdinMenuItem PrevVisualMenuItem
        {
            get
            {
                this.EnsureInitialized();

                // Performance optimization:
                if (this.ChildMenuItems.Count > 0 && this.Toggled == false && this.IsVisibleRecrusive())
                {
                    if (this.previousMenuItem != null)
                    {
                        if (this.previousMenuItem.ChildMenuItems.Count == 0 || this.previousMenuItem.Toggled == false)
                        {
                            if (this.previousMenuItem.IsVisible)
                            {
                                return this.previousMenuItem;
                            }
                        }
                    }
                    else if (this.parentMenuItem != null && this.parentMenuItem.IsVisible)
                    {
                        return this.parentMenuItem;
                    }
                }

                // Bruteforce search:
                return this.GetAllPreviousMenuItems().FirstOrDefault(x => x.IsVisibleRecrusive());
            }
        }

        /// <summary>
        /// Gets the drawn rect.
        /// </summary>
        public Rect Rect
        {
            get { return this.rect; }
        }

        /// <summary>
        /// Gets the drawn label rect.
        /// </summary>
        public Rect LabelRect
        {
            get { return this.labelRect; }
        }

        /// <summary>
        /// Gets or sets the style. If null is specified, then the menu trees DefaultMenuStyle is used.
        /// </summary>
        public OdinMenuStyle Style
        {
            get { return this.style ?? this.menuTree.DefaultMenuStyle; }
            set { this.style = value; }
        }

        /// <summary>
        /// Deselects this instance.
        /// </summary>
        public bool Deselect()
        {
            return this.menuTree.Selection.Remove(this);
        }

        /// <summary>
        /// Selects the specified add to selection.
        /// </summary>
        public void Select(bool addToSelection = false)
        {
            if (addToSelection == false)
            {
                this.menuTree.Selection.Clear();
            }

            this.menuTree.Selection.Add(this);
        }

        /// <summary>
        /// Gets the child menu items recursive in a DFS.
        /// </summary>
        /// <param name="includeSelf">Whether to include it self in the collection.</param>
        public IEnumerable<OdinMenuItem> GetChildMenuItemsRecursive(bool includeSelf)
        {
            if (includeSelf) yield return this;

            foreach (var child in this.ChildMenuItems.SelectMany(x => x.GetChildMenuItemsRecursive(true)))
            {
                yield return child;
            }
        }

        /// <summary>
        /// Gets the child menu items recursive in a DFS.
        /// </summary>
        /// <param name="includeSelf">Whether to include it self in the collection.</param>
        public IEnumerable<OdinMenuItem> GetParentMenuItemsRecursive(bool includeSelf, bool includeRoot = false)
        {
            if (includeSelf || this.Parent == null && includeRoot)
            {
                yield return this;
            }

            if (this.Parent != null)
            {
                foreach (var item in this.Parent.GetParentMenuItemsRecursive(true, includeRoot))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Gets the full menu item path.
        /// </summary>
        public string GetFullPath()
        {
            this.EnsureInitialized();

            var parent = this.Parent;

            if (parent == null)
            {
                return this.SmartName;
            }

            return parent.GetFullPath() + "/" + this.SmartName;
        }

        /// <summary>
        /// Gets the first object of the <see cref="ObjectInstances"/>
        /// </summary>
        public virtual object ObjectInstance
        {
            get
            {
                if (this.objectInstances == null)
                {
                    return null;
                }

                if (this.objectInstances.Count == 0)
                {
                    return null;
                }

                var instance = this.objectInstances[0];
                var instanceFunc = instance as Func<object>;

                if (instanceFunc != null)
                {
                    return instanceFunc.Invoke();
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets the object instances the menu item represents
        /// </summary>
        public virtual IEnumerable<object> ObjectInstances
        {
            get
            {
                if (this.objectInstances == null)
                {
                    yield break;
                }

                foreach (var item in this.objectInstances)
                {
                    if (item == null)
                    {
                        yield return null;
                    }

                    if (this.objectInstances.Count == 0)
                    {
                        yield return null;
                    }

                    var instance = item;
                    var instanceFunc = instance as Func<object>;
                    if (instanceFunc != null)
                    {
                        yield return instanceFunc.Invoke();
                    }

                    yield return instance;
                }
            }
        }

        /// <summary>
        /// Sets the object instance
        /// </summary>
        public void SetObjectInstance(object obj)
        {
            this.objectInstances = new object[] { obj };
        }

        /// <summary>
        /// Sets the object instances
        /// </summary>
        public void SetObjectInstances(IList objects)
        {
            this.objectInstances = objects;
        }

        /// <summary>
        /// Gets a nice menu item name. If the raw name value is null or a dollar sign, then the name is retrieved from the object itself.
        /// </summary>
        public virtual string SmartName
        {
            get
            {
                if (this.name == null || this.name == "$")
                {
                    if (this.ObjectInstance == null)
                    {
                        return "";
                    }

                    var unityObject = this.ObjectInstance as UnityEngine.Object;

                    if (unityObject)
                    {
                        return unityObject.name.SplitPascalCase();
                    }

                    return this.ObjectInstance.ToString();
                }

                // Both object instances, and the name can change, and if the menu item name is a reference to a member inside the object instance
                // then we also need to make a new string member helper in those cases as well.

                bool makeNewGetter = false;
                if (this.nameValueGetter == null)
                {
                    makeNewGetter = true;
                }
                else if (this.prevName != this.name)
                {
                    makeNewGetter = true;
                    this.prevName = this.name;
                }
                else if (this.nameValueGetter != null && this.ObjectInstance != null && this.nameValueGetter.ObjectType != this.ObjectInstance.GetType())
                {
                    makeNewGetter = true;
                }

                if (this.ObjectInstance == null)
                {
                    this.nameValueGetter = null;
                }
                else if (makeNewGetter)
                {
                    this.nameValueGetter = new StringMemberHelper(this.ObjectInstance.GetType(), this.name);
                }

                if (this.nameValueGetter != null)
                {
                    return this.nameValueGetter.ForceGetString(this.ObjectInstance);
                }
                else
                {
                    return this.name;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="OdinMenuItem"/> is toggled / expanded. This value tries it best to be persistent.
        /// </summary>
        public virtual bool Toggled
        {
            get
            {
                if (this.isToggledContext == null)
                {
                    this.isToggledContext = LocalPersistentContext<bool>.Create(PersistentContext.Get("[OdinMenuItem]" + this.GetFullPath(), this.DefaultToggledState));
                }

                return this.isToggledContext.Value;
            }
            set
            {
                if (this.isToggledContext == null)
                {
                    this.isToggledContext = LocalPersistentContext<bool>.Create(PersistentContext.Get("[OdinMenuItem]" + this.GetFullPath(), this.DefaultToggledState));
                }

                this.isToggledContext.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is foldable.
        /// </summary>
        [Obsolete("This was previosly only hiding the triangle. This can be done by giving the menu item a custom style: menuItem.Style = menuItem.Style.Clone().SetDrawFoldoutTriangle(value);")]
        public bool IsFoldable
        {
            get { return false; }
            set { }
        }

        private float t = -1;
        private bool wasMouseDownEvent;

        /// <summary>
        /// Draws this menu item followed by all of its child menu items
        /// </summary>
        /// <param name="indentLevel">The indent level.</param>
        public virtual void DrawMenuItems(int indentLevel)
        {
            if (this.IsVisible)
            {
                this.DrawMenuItem(indentLevel);
            }

            if (this.MenuTree.DrawInSearchMode)
            {
                for (int i = 0; i < this.ChildMenuItems.Count; i++)
                {
                    var child = this.ChildMenuItems[i];
                    child.DrawMenuItems(indentLevel);
                }
            }
            else
            {
                bool isVisible = this.Toggled;

                if (t < 0)
                {
                    t = this.Toggled ? 1 : 0;
                }

                if (Event.current.type == EventType.Layout)
                {
                    t = Mathf.MoveTowards(t, isVisible ? 1 : 0, EditorTimeHelper.Time.DeltaTime * (1f / SirenixEditorGUI.DefaultFadeGroupDuration));
                }

                if (SirenixEditorGUI.BeginFadeGroup(t))
                {
                    for (int i = 0; i < this.ChildMenuItems.Count; i++)
                    {
                        var child = this.ChildMenuItems[i];
                        child.DrawMenuItems(indentLevel + 1);
                    }
                }
                SirenixEditorGUI.EndFadeGroup();
            }
        }

        /// <summary>
        /// Draws the menu item with the specified indent level.
        /// </summary>
        public virtual void DrawMenuItem(int indentLevel)
        {
            var newRect = GUILayoutUtility.GetRect(0, this.Style.Height);

            if (Event.current.type == EventType.Repaint || Event.current.type != EventType.Layout && this.rect.width == 0)
            {
                this.rect = newRect;
            }

            if (Event.current.type == EventType.Repaint)
            {
                float indent = this.Style.Offset + (indentLevel * this.Style.IndentAmount);
                this.labelRect = this.rect.AddXMin(indent);
                var selected = this.IsSelected;

                // bg
                if (selected)
                {
                    if (EditorGUIUtility.isProSkin)
                    {
                        EditorGUI.DrawRect(this.rect, this.Style.SelectedColorDarkSkin);
                    }
                    else
                    {
                        EditorGUI.DrawRect(this.rect, this.Style.SelectedColorLightSkin);
                    }
                }

                // Hover
                if (!selected && this.rect.Contains(Event.current.mousePosition))
                {
                    EditorGUI.DrawRect(this.rect, mouseOverColor);
                }

                // Triangle
                if (this.ChildMenuItems.Count > 0 && !this.MenuTree.DrawInSearchMode && this.Style.DrawFoldoutTriangle)
                {
                    var icon = this.Toggled ? EditorIcons.TriangleDown : EditorIcons.TriangleRight;

                    if (this.Style.AlignTriangleLeft)
                    {
                        this.triangleRect = this.labelRect.AlignLeft(this.Style.TriangleSize).AlignMiddle(this.Style.TriangleSize);
                        this.triangleRect.x -= this.Style.TriangleSize - this.Style.TrianglePadding;
                    }
                    else
                    {
                        this.triangleRect = this.rect.AlignRight(this.Style.TriangleSize).AlignMiddle(this.Style.TriangleSize);
                        this.triangleRect.x -= this.Style.TrianglePadding;
                    }

                    if (Event.current.type == EventType.Repaint)
                    {
                        if (EditorGUIUtility.isProSkin)
                        {
                            if (selected || this.triangleRect.Contains(Event.current.mousePosition))
                            {
                                GUI.DrawTexture(this.triangleRect, icon.Highlighted);
                            }
                            else
                            {
                                GUI.DrawTexture(this.triangleRect, icon.Active);
                            }
                        }
                        else
                        {
                            if (selected)
                            {
                                GUI.DrawTexture(this.triangleRect, icon.Raw);
                            }
                            else if (this.triangleRect.Contains(Event.current.mousePosition))
                            {
                                GUI.DrawTexture(this.triangleRect, icon.Active);
                            }
                            else
                            {
                                GUIHelper.PushColor(new Color(1, 1, 1, 0.7f));
                                GUI.DrawTexture(this.triangleRect, icon.Active);
                                GUIHelper.PopColor();
                            }
                        }
                    }
                }

                // Icon
                if (this.Icon || selected && this.IconSelected)
                {
                    var iconRect = this.labelRect.AlignLeft(this.Style.IconSize).AlignMiddle(this.Style.IconSize);
                    iconRect.x += this.Style.IconOffset;
                    if (!selected)
                    {
                        GUIHelper.PushColor(new Color(1, 1, 1, this.Style.NotSelectedIconAlpha));
                    }

                    if (selected && this.IconSelected)
                    {
                        GUI.DrawTexture(iconRect, this.IconSelected);
                    }
                    else
                    {
                        GUI.DrawTexture(iconRect, this.Icon);
                    }

                    this.labelRect.xMin += this.Style.IconSize + this.Style.IconPadding;

                    if (!selected)
                    {
                        GUIHelper.PopColor();
                    }
                }

                // Label
                var labelStyle = selected ? SirenixGUIStyles.WhiteLabel : SirenixGUIStyles.Label;
                this.labelRect = this.labelRect.AlignMiddle(16);
                GUI.Label(this.labelRect, this.SmartName, labelStyle);

                // Borders
                if (this.Style.Borders)
                {
                    var borderPadding = this.Style.BorderPadding;
                    var draw = true;

                    if (selected || previousMenuItemWasSelected)
                    {
                        borderPadding = 0;
                        if (!EditorGUIUtility.isProSkin)
                        {
                            draw = false;
                        }
                    }

                    previousMenuItemWasSelected = selected;

                    if (draw)
                    {
                        var border = this.rect;
                        border.x += borderPadding;
                        border.width -= borderPadding * 2;
                        SirenixEditorGUI.DrawHorizontalLineSeperator(border.x, border.y, border.width, this.Style.BorderAlpha);
                    }
                }
            }

            // Prepare mouse stuff.
            // If the user uses the MouseDown event to for instance start a drag.
            // The the menu item won't be selected, this fixes it so that we also listen to the mouse up event.

            // TODO: Add the concept of a menuitem being highligted, and change the click behaviour so that mouse down highlights,
            // and mouse up selects. On a related note.. also give DragAndDropUtilities.DragZone 
            // a bit af distance threshold before a drag actually starts, so that it becomes a little easier to click something
            // without starting a drag.
            this.wasMouseDownEvent = Event.current.type == EventType.MouseDown && this.rect.Contains(Event.current.mousePosition);
            if (this.wasMouseDownEvent)
            {
                handleClickEventOnMouseUp = this;
            }

            // Draw user stuff.
            this.OnDrawMenuItem(this.rect, this.labelRect);

            if (this.OnDrawItem != null)
            {
                this.OnDrawItem(this);
            }

            // Handle mouse events.
            this.HandleMouseEvents(rect, this.triangleRect);
        }

        /// <summary>
        /// Override this to add custom GUI to the menu items.
        /// This is called right after the menu item is done drawing, and right before mouse input is handles so you can take control of that.
        /// </summary>
        protected virtual void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
        }

        internal void UpdateMenuTreeRecursive(bool isRoot = false)
        {
            this.isInitialized = true;
            OdinMenuItem prev = null;
            foreach (var child in this.ChildMenuItems)
            {
                if (!isRoot)
                {
                    child.parentMenuItem = this;
                }

                if (prev != null)
                {
                    prev.nextMenuItem = child;
                    child.previousMenuItem = prev;
                }

                prev = child;

                child.UpdateMenuTreeRecursive();
            }

            if (isRoot)
            {
                int i = 0;
                prev = null;
                foreach (var item in this.menuTree.EnumerateTree())
                {
                    item.flatTreeIndex = i++;
                    if (prev != null)
                    {
                        item.previousMenuItemFlat = prev;
                        prev.nextMenuItemFlat = item;
                    }
                    prev = item;
                }
            }
        }

        /// <summary>
        /// Handles the mouse events.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="triangleRect">The triangle rect.</param>
        protected void HandleMouseEvents(Rect rect, Rect triangleRect)
        {
            var e = Event.current.type;

            if (e == EventType.Used && this.wasMouseDownEvent)
            {
                this.wasMouseDownEvent = false;
                handleClickEventOnMouseUp = this;
            }

            bool isMouseClick =
                (e == EventType.MouseDown) ||
                (e == EventType.MouseUp && handleClickEventOnMouseUp == this);

            if (isMouseClick)
            {
                handleClickEventOnMouseUp = null;
                this.wasMouseDownEvent = false;

                if (!rect.Contains(Event.current.mousePosition))
                {
                    return;
                }

                var hasChildren = this.ChildMenuItems.Any();
                var instance = this.ObjectInstance;
                var selected = this.IsSelected;
                var selectable = !triangleRect.Contains(Event.current.mousePosition);

                bool isUnityObjectInstance = instance as UnityEngine.Object;

                if (selected && isUnityObjectInstance)
                {
                    var unityObject = instance as UnityEngine.Object;
                    var behaviour = unityObject as Behaviour;
                    if (behaviour)
                    {
                        unityObject = behaviour.gameObject;
                    }
                    EditorGUIUtility.PingObject(unityObject);
                }

                if (Event.current.button == 1)
                {
                    if (this.OnRightClick != null)
                    {
                        this.OnRightClick(this);
                    }
                }

                if (Event.current.button == 0)
                {
                    if (hasChildren && selected && Event.current.modifiers == EventModifiers.None || !selectable)
                    {
                        this.Toggled = !this.Toggled;
                    }

                    if (!selectable && Event.current.modifiers == EventModifiers.Alt)
                    {
                        selectable = true;
                    }

                    if (selectable)
                    {
                        if (Event.current.modifiers == EventModifiers.Alt)
                        {
                            foreach (var item in this.GetChildMenuItemsRecursive(false))
                            {
                                item.Toggled = this.Toggled;
                            }
                        }

                        bool shiftSelect =
                            this.menuTree.Selection.SupportsMultiSelect &&
                            Event.current.modifiers == EventModifiers.Shift &&
                            this.menuTree.Selection.Count > 0;

                        if (shiftSelect)
                        {
                            var curr = this.menuTree.Selection.First();
                            var maxIterations = Mathf.Abs(curr.FlatTreeIndex - this.FlatTreeIndex) + 1;
                            var down = curr.FlatTreeIndex < this.FlatTreeIndex;
                            this.menuTree.Selection.Clear();

                            for (int i = 0; i < maxIterations; i++)
                            {
                                if (curr == null)
                                {
                                    break;
                                }

                                curr.Select(true);

                                if (curr == this)
                                {
                                    break;
                                }

                                curr = down ? curr.NextVisualMenuItem : curr.PrevVisualMenuItem;
                            }
                        }
                        else
                        {
                            var ctrl = Event.current.modifiers == EventModifiers.Control;
                            if (ctrl && selected)
                            {
                                this.Deselect();
                            }
                            else
                            {
                                this.Select(ctrl);
                            }

                            if (Event.current.clickCount == 2)
                            {
                                this.MenuTree.Selection.ConfirmSelection();
                            }
                        }
                    }
                }

                GUIHelper.RemoveFocusControl();
                Event.current.Use();
            }
        }

        internal bool IsVisibleRecrusive()
        {
            if (this.MenuTree.DrawInSearchMode)
            {
                return this.IsVisible;
            }

            return this.ParentMenuItemsBottomUp(false).Any(x => x.Toggled == false) == false;
        }

        private IEnumerable<OdinMenuItem> GetAllNextMenuItems()
        {
            if (this.nextMenuItemFlat != null)
            {
                yield return this.nextMenuItemFlat;

                foreach (var item in this.nextMenuItemFlat.GetAllNextMenuItems())
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<OdinMenuItem> GetAllPreviousMenuItems()
        {
            if (this.previousMenuItemFlat != null)
            {
                yield return this.previousMenuItemFlat;

                foreach (var item in this.previousMenuItemFlat.GetAllPreviousMenuItems())
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<OdinMenuItem> ParentMenuItemsBottomUp(bool includeSelf = true)
        {
            if (this.parentMenuItem != null)
            {
                foreach (var item in this.parentMenuItem.ParentMenuItemsBottomUp())
                {
                    yield return item;
                }
            }

            if (includeSelf)
            {
                yield return this;
            }
        }

        private void EnsureInitialized()
        {
            if (this.isInitialized == false)
            {
                this.menuTree.UpdateMenuTree();

                if (this.isInitialized == false)
                {
                    Debug.LogWarning("Could not initialize menu item. Is the menu item not part of a menu tree?");
                }
            }
        }
    }
}
#endif