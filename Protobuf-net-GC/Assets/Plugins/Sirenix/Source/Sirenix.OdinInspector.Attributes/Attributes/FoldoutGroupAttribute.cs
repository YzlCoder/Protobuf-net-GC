//-----------------------------------------------------------------------
// <copyright file="FoldoutGroupAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// <para>FoldoutGroup is used on any property, and organizes properties into a foldout.</para>
    /// <para>Use this to organize properties, and to allow the user to hide properties that are not relevant for them at the moment.</para>
    /// </summary>
    /// <example>
    /// <para>The following example shows how FoldoutGroup is used to organize properties into a foldout.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    ///	{
    ///		[FoldoutGroup("MyGroup")]
    ///		public int A;
    ///
    ///		[FoldoutGroup("MyGroup")]
    ///		public int B;
    ///
    ///		[FoldoutGroup("MyGroup")]
    ///		public int C;
    ///	}
    /// </code>
    /// </example>
    /// <example>
    /// <para>The following example shows how properties can be organizes into multiple foldouts.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    ///	{
    ///		[FoldoutGroup("First")]
    ///		public int A;
    ///
    ///		[FoldoutGroup("First")]
    ///		public int B;
    ///
    ///		[FoldoutGroup("Second")]
    ///		public int C;
    ///	}
    /// </code>
    /// </example>
    /// <seealso cref="BoxGroupAttribute"/>
    /// <seealso cref="ButtonGroupAttribute"/>
    /// <seealso cref="TabGroupAttribute"/>
    /// <seealso cref="ToggleGroupAttribute"/>
    /// <seealso cref="PropertyGroupAttribute"/>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class FoldoutGroupAttribute : PropertyGroupAttribute
    {
        /// <summary>
        /// Adds the property to the specified foldout group.
        /// </summary>
        /// <param name="groupName">Name of the foldout group.</param>
        /// <param name="order">The order of the group in the inspector.</param>
        public FoldoutGroupAttribute(string groupName, int order = 0)
            : base(groupName, order)
        {
        }

        /// <summary>
        /// Adds the property to the specified foldout group.
        /// </summary>
        /// <param name="groupName">Name of the foldout group.</param>
        /// <param name="expanded">Whether or not the foldout should be expanded by default.</param>
        /// <param name="order">The order of the group in the inspector.</param>
        public FoldoutGroupAttribute(string groupName, bool expanded, int order = 0)
            : base(groupName, order)
        {
            this.Expanded = expanded;
            this.HasDefinedExpanded = true;
        }

        /// <summary>
        /// Adds the property to the specified foldout group.
        /// </summary>
        /// <param name="groupName">Name of the foldout group.</param>
        /// <param name="titleStringMemberName">Obsolete.</param>
        /// <param name="order">The order of the group in the inspector.</param>
        [Obsolete("Use [FoldoutGroup(\"$MemberName\")] instead.")]
        public FoldoutGroupAttribute(string groupName, string titleStringMemberName, int order = 0)
            : base(groupName, order)
        {
            //this.TitleStringMemberName = titleStringMemberName;
        }

        /// <summary>
        /// Combines the foldout property with another.
        /// </summary>
        /// <param name="other">The group to combine with.</param>
        protected override void CombineValuesWith(PropertyGroupAttribute other)
        {
            var attr = other as FoldoutGroupAttribute;
            if (attr.HasDefinedExpanded)
            {
                this.HasDefinedExpanded = true;
                this.Expanded = attr.Expanded;
            }

            if (this.HasDefinedExpanded)
            {
                attr.HasDefinedExpanded = true;
                attr.Expanded = this.Expanded;
            }
        }

        /// <summary>
        /// Name of any string field, property or function, to title the foldout in the inspector.
        /// </summary>
        [Obsolete("Use [FoldoutGroup(\"$MemberName\")] instead.")]
        public string TitleStringMemberName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether or not the foldout should be expanded by default..
        /// </summary>
        public bool Expanded { get; private set; }

        /// <summary>
        /// Gets a value indicating whether or not the Expanded property has been set.
        /// </summary>
        public bool HasDefinedExpanded { get; private set; }
    }
}