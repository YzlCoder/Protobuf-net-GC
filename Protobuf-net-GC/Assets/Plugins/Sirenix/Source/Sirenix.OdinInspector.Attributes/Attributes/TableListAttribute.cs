//-----------------------------------------------------------------------
// <copyright file="TableListAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// Renders lists and arrays in the inspector as tables.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class TableListAttribute : Attribute
    {
        /// <summary>
        /// Override the default setting specified in the Advanced Odin Preferences window and explicitly tells how many items each page should contain.
        /// </summary>
        public int NumberOfItemsPerPage { get; set; }

        /// <summary>
        /// Mark the table as read-only. This removes all editing capabilities from the list such as Add and delete,
        /// but without disabling GUI for each element drawn as otherwise would be the case if the <see cref="ReadOnlyAttribute"/> was used.
        /// </summary>
        public bool IsReadOnly { get; set; }
    }
}