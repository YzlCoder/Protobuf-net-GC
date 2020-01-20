#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EditorIconsOverview.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
    using UnityEngine;
    using UnityEditor;
    using Sirenix.Utilities.Editor;
    using Sirenix.Utilities;
    using System.Linq;

    public class EditorIconsOverview : OdinSelector<object>
    {
        [MenuItem("Tools/Odin Inspector/Odin Editor Icons")]
        public static void OpenOverivew()
        {
            var window = OdinEditorWindow.InspectObject(new EditorIconsOverview());
            window.ShowUtility();
            window.WindowPadding = new Vector4();
        }

        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            this.DrawConfirmSelectionButton = false;
            tree.Config.DrawSearchToolbar = true;
            tree.DefaultMenuStyle.Height = 25;

            foreach (var item in typeof(EditorIcons).GetProperties(Flags.StaticPublic).OrderBy(x => x.Name))
            {
                var returnType = item.GetReturnType();

                if (typeof(Texture).IsAssignableFrom(returnType))
                {
                    tree.Add(item.Name, null, (Texture)item.GetGetMethod().Invoke(null, null));
                }
                else if (typeof(EditorIcon).IsAssignableFrom(returnType))
                {
                    tree.Add(item.Name, null, (EditorIcon)item.GetGetMethod().Invoke(null, null));
                }
            }
        }

        [ShowInInspector, PropertyOrder(30)]
        [PropertyRange(10, 34), LabelWidth(50)]
        [InfoBox("This is an overview of all available icons in the Sirenix.Utilities.Editor.EditorIcons utility class.")]
        private float Size
        {
            get { return this.SelectionTree.DefaultMenuStyle.IconSize; }
            set
            {
                this.SelectionTree.DefaultMenuStyle.IconSize = value;
                this.SelectionTree.DefaultMenuStyle.Height = (int)value + 9;
            }
        }
    }
}
#endif