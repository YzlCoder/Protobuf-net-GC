﻿//-----------------------------------------------------------------------
// <copyright file="FilePathAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
	using System;
	
	/// <summary>
	/// <para>FilePath is used on string properties, and provides an interface for file paths.</para>
	/// </summary>
	/// <example>
	/// <para>The following example demonstrates how FilePath is used.</para>
	/// <code>
	///	public class FilePathExamples : MonoBehaviour
	///	{
	///		// By default, FilePath provides a path relative to the Unity project.
	///		[FilePath]
	///		public string UnityProjectPath;
	///	
	///		// It is possible to provide custom parent path. Parent paths can be relative to the Unity project, or absolute.
	///		[FilePath(ParentFolder = "Assets/Plugins/Sirenix")]
	///		public string RelativeToParentPath;
	///	
	///		// Using parent path, FilePath can also provide a path relative to a resources folder.
	///		[FilePath(ParentFolder = "Assets/Resources")]
	///		public string ResourcePath;
	///	
	///		// Provide a comma seperated list of allowed extensions. Dots are optional.
	///		[FilePath(Extensions = "cs")]
	///		public string ScriptFiles;
	///	
	///		// By setting AbsolutePath to true, the FilePath will provide an absolute path instead.
	///		[FilePath(AbsolutePath = true)]
	///		[BoxGroup("Conditions")]
	///		public string AbsolutePath;
	///	
	///		// FilePath can also be configured to show an error, if the provided path is invalid.
	///		[FilePath(RequireValidPath = true)]
	///		public string ValidPath;
	///	
	///		// By default, FilePath will enforce the use of forward slashes. It can also be configured to use backslashes instead.
	///		[FilePath(UseBackslashes = true)]
	///		public string Backslashes;
	///	
	///		// FilePath also supports member references with the $ symbol.
	///		[FilePath(ParentFolder = "$DynamicParent", Extensions = "$DynamicExtensions")]
	///		public string DynamicFilePath;
	///	
	///		public string DynamicParent = "Assets/Plugin/Sirenix";
	///	
	///		public string DynamicExtensions = "cs, unity, jpg";
	///	}
	/// </code>
	/// </example>
	/// <seealso cref="FolderPathAttribute"/>
	/// <seealso cref="DisplayAsStringAttribute"/>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class FilePathAttribute : Attribute
	{
		/// <summary>
		/// If <c>true</c> the FilePath will provide an absolute path, instead of a relative one.
		/// </summary>
		public bool AbsolutePath { get; set; }

		/// <summary>
		/// Comma seperated list of allowed file extensions. Dots are optional.
		/// Supports member referencing with $.
		/// </summary>
		public string Extensions { get; set; }
		
		/// <summary>
		/// ParentFolder provides an override for where the path is relative to. ParentFolder can be relative to the Unity project, or an absolute path.
		/// Supports member referencing with $.
		/// </summary>
		public string ParentFolder { get; set; }

		/// <summary>
		/// If <c>true</c> an error will be displayed for invalid, or missing paths.
		/// </summary>
		public bool RequireValidPath { get; set; }

        /// <summary>
        /// By default FilePath enforces forward slashes. Set UseBackslashes to <c>true</c> if you want backslashes instead.
        /// </summary>
        public bool UseBackslashes { get; set; }

        /// <summary>
        /// Whether the path should read only.
        /// </summary>
        [Obsolete("Add a ReadOnly attribute to the property instead.")]
        public bool ReadOnly { get; set; }
    }
}