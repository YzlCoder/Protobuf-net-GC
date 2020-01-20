#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ProgressBarAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using System;
    using System.Reflection;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    internal class ProgressBarContext<T>
    {
        public string ErrorMessage;
        public Func<Color> StaticColorGetter;
        public Func<object, Color> InstanceColorGetter;
        public Func<object, Color> InstanceColorMethod;
        public Func<object, T, Color> InstanceColorParameterMethod;
        public Func<Color> StaticBackgroundColorGetter;
        public Func<object, Color> InstanceBackgroundColorGetter;
        public Func<object, Color> InstanceBackgroundColorMethod;
        public Func<object, T, Color> InstanceBackgroundColorParameterMethod;
        public StringMemberHelper CustomValueLabelGetter;

        public ProgressBarConfig GetConfig(IPropertyValueEntry<T> entry, ProgressBarAttribute attribute)
        {
            var config = ProgressBarConfig.Default;
            config.Height = attribute.Height;
            config.DrawValueLabel = attribute.DrawValueLabelHasValue ? attribute.DrawValueLabel : (attribute.Segmented ? false : true);
            config.ValueLabelAlignment = attribute.ValueLabelAlignmentHasValue ? attribute.ValueLabelAlignment : (attribute.Segmented ? TextAlignment.Right : TextAlignment.Center);

            if (attribute.CustomValueStringMember != null)
            {
                // Do not draw default label.
                config.DrawValueLabel = false;
            }

            // No point in updating the color in non-repaint events.
            if (Event.current.type == EventType.Repaint)
            {
                var parent = entry.Property.FindParent(PropertyValueCategory.Member, true).ParentValues[0];

                config.ForegroundColor =
                    this.StaticColorGetter != null ? this.StaticColorGetter() :
                    this.InstanceColorGetter != null ? this.InstanceColorGetter(parent) :
                    this.InstanceColorMethod != null ? this.InstanceColorMethod(parent) :
                    this.InstanceColorParameterMethod != null ? this.InstanceColorParameterMethod(parent, entry.SmartValue) :
                    new Color(attribute.R, attribute.G, attribute.B, 1f);

                config.BackgroundColor =
                    this.StaticBackgroundColorGetter != null ? this.StaticBackgroundColorGetter() :
                    this.InstanceBackgroundColorGetter != null ? this.InstanceBackgroundColorGetter(parent) :
                    this.InstanceBackgroundColorMethod != null ? this.InstanceBackgroundColorMethod(parent) :
                    this.InstanceBackgroundColorParameterMethod != null ? this.InstanceBackgroundColorParameterMethod(parent, entry.SmartValue) :
                    config.BackgroundColor; // Use default if no other option available.
            }

            return config;
        }
    }

    /// <summary>
    /// Common base implementation for progress bar attribute drawers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseProgressBarAttributeDrawer<T> : OdinAttributeDrawer<ProgressBarAttribute, T>
    {
        internal static ProgressBarContext<T> GetContext(OdinDrawer drawer, IPropertyValueEntry<T> entry, ProgressBarAttribute attribute)
        {
            PropertyContext<ProgressBarContext<T>> context;
            if (entry.Context.Get(drawer, "ProgressBarContext", out context))
            {
                context.Value = new ProgressBarContext<T>();
                var parentType = entry.Property.FindParent(PropertyValueCategory.Member, true).ParentType;

                // Foreground color member.
                if (!attribute.ColorMember.IsNullOrWhitespace())
                {
                    MemberInfo member;
                    if (MemberFinder.Start(parentType)
                        .IsNamed(attribute.ColorMember)
                        .HasReturnType<Color>()
                        .TryGetMember(out member, out context.Value.ErrorMessage))
                    {
                        if (member is FieldInfo || member is PropertyInfo)
                        {
                            if (member.IsStatic())
                            {
                                context.Value.StaticColorGetter = DeepReflection.CreateValueGetter<Color>(parentType, attribute.ColorMember);
                            }
                            else
                            {
                                context.Value.InstanceColorGetter = DeepReflection.CreateWeakInstanceValueGetter<Color>(parentType, attribute.ColorMember);
                            }
                        }
                        else if (member is MethodInfo)
                        {
                            if (member.IsStatic())
                            {
                                context.Value.ErrorMessage = "Static method members are currently not supported.";
                            }
                            else
                            {
                                var method = member as MethodInfo;
                                var p = method.GetParameters();

                                if (p.Length == 0)
                                {
                                    context.Value.InstanceColorMethod = EmitUtilities.CreateWeakInstanceMethodCallerFunc<Color>(method);
                                }
                                else if (p.Length == 1 && p[0].ParameterType == typeof(T))
                                {
                                    context.Value.InstanceColorParameterMethod = EmitUtilities.CreateWeakInstanceMethodCallerFunc<T, Color>(method);
                                }
                            }
                        }
                        else
                        {
                            context.Value.ErrorMessage = "Unsupported member type.";
                        }
                    }
                }

                // Background color member.
                if (!attribute.BackgroundColorMember.IsNullOrWhitespace())
                {
                    MemberInfo member;
                    if (MemberFinder.Start(parentType)
                        .IsNamed(attribute.BackgroundColorMember)
                        .HasReturnType<Color>()
                        .TryGetMember(out member, out context.Value.ErrorMessage))
                    {
                        if (member is FieldInfo || member is PropertyInfo)
                        {
                            if (member.IsStatic())
                            {
                                context.Value.StaticBackgroundColorGetter = DeepReflection.CreateValueGetter<Color>(parentType, attribute.BackgroundColorMember);
                            }
                            else
                            {
                                context.Value.InstanceBackgroundColorGetter = DeepReflection.CreateWeakInstanceValueGetter<Color>(parentType, attribute.BackgroundColorMember);
                            }
                        }
                        else if (member is MethodInfo)
                        {
                            if (member.IsStatic())
                            {
                                context.Value.ErrorMessage = "Static method members are currently not supported.";
                            }
                            else
                            {
                                var method = member as MethodInfo;
                                var p = method.GetParameters();

                                if (p.Length == 0)
                                {
                                    context.Value.InstanceBackgroundColorMethod = EmitUtilities.CreateWeakInstanceMethodCallerFunc<Color>(method);
                                }
                                else if (p.Length == 1 && p[0].ParameterType == typeof(T))
                                {
                                    context.Value.InstanceBackgroundColorParameterMethod = EmitUtilities.CreateWeakInstanceMethodCallerFunc<T, Color>(method);
                                }
                            }
                        }
                        else
                        {
                            context.Value.ErrorMessage = "Unsupported member type.";
                        }
                    }
                }

                // Custom value string getter
                if (attribute.CustomValueStringMember != null && attribute.CustomValueStringMember.Length > 0)
                {
                    string member = attribute.CustomValueStringMember;
                    if (attribute.CustomValueStringMember[0] != '$')
                    {
                        member = "$" + attribute.CustomValueStringMember;
                    }

                    context.Value.CustomValueLabelGetter = new StringMemberHelper(
                        entry.Property.ParentType,
                        member,
                        ref context.Value.ErrorMessage);
                }
            }

            return context.Value;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, ProgressBarAttribute attribute, GUIContent label)
        {
            ProgressBarContext<T> context = GetContext(this, entry, attribute);

            // Display evt. error
            if (context.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(context.ErrorMessage);
            }

            ProgressBarConfig config = context.GetConfig(entry, attribute);

            // Construct a Rect based on the configured height of the field.
            Rect rect = EditorGUILayout.GetControlRect(label != null, config.Height < EditorGUIUtility.singleLineHeight ? EditorGUIUtility.singleLineHeight : config.Height);

            // Draw the field.
            EditorGUI.BeginChangeCheck();
            T value = this.DrawProgressBar(rect, label, entry.SmartValue, attribute, config, context.CustomValueLabelGetter != null ? context.CustomValueLabelGetter.GetString(entry) : null);

            // Apply evt. changes
            if (EditorGUI.EndChangeCheck())
            {
                entry.SmartValue = value;
            }
        }

        /// <summary>
        /// Generic implementation of progress bar field drawing.
        /// </summary>
        protected abstract T DrawProgressBar(Rect rect, GUIContent label, T value, ProgressBarAttribute attribute, ProgressBarConfig config, string valueLabel);
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    [OdinDrawer]
    public sealed class ProgressBarAttributeByteDrawer : BaseProgressBarAttributeDrawer<byte>
    {
        /// <summary>
        /// Draws a progress bar for a byte property.
        /// </summary>
        protected override byte DrawProgressBar(Rect rect, GUIContent label, byte value, ProgressBarAttribute attribute, ProgressBarConfig config, string valueLabel)
        {
            if (attribute.Segmented)
            {
                return (byte)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)value, (long)attribute.Min, (long)attribute.Max, config, valueLabel);
            }
            else
            {
                return (byte)SirenixEditorFields.ProgressBarField(rect, label, (double)value, attribute.Min, attribute.Max, config, valueLabel);
            }
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    [OdinDrawer]
    public sealed class ProgressBarAttributeSbyteDrawer : BaseProgressBarAttributeDrawer<sbyte>
    {
        /// <summary>
        /// Draws a progress bar for a sbyte property.
        /// </summary>
        protected override sbyte DrawProgressBar(Rect rect, GUIContent label, sbyte value, ProgressBarAttribute attribute, ProgressBarConfig config, string valueLabel)
        {
            if (attribute.Segmented)
            {
                return (sbyte)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)value, (long)attribute.Min, (long)attribute.Max, config, valueLabel);
            }
            else
            {
                return
                    (sbyte)SirenixEditorFields.ProgressBarField(rect, label, (double)value, attribute.Min, attribute.Max, config, valueLabel);
            }
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    [OdinDrawer]
    public sealed class ProgressBarAttributeShortDrawer : BaseProgressBarAttributeDrawer<short>
    {
        /// <summary>
        /// Draws a progress bar for a short property.
        /// </summary>
        protected override short DrawProgressBar(Rect rect, GUIContent label, short value, ProgressBarAttribute attribute, ProgressBarConfig config, string valueLabel)
        {
            if (attribute.Segmented)
            {
                return (short)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)value, (long)attribute.Min, (long)attribute.Max, config, valueLabel);
            }
            else
            {
                return (short)SirenixEditorFields.ProgressBarField(rect, label, (double)value, attribute.Min, attribute.Max, config, valueLabel);
            }
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    [OdinDrawer]
    public sealed class ProgressBarAttributeUshortDrawer : BaseProgressBarAttributeDrawer<ushort>
    {
        /// <summary>
        /// Draws a progress bar for a ushort property.
        /// </summary>
        protected override ushort DrawProgressBar(Rect rect, GUIContent label, ushort value, ProgressBarAttribute attribute, ProgressBarConfig config, string valueLabel)
        {
            if (attribute.Segmented)
            {
                return (ushort)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)value, (long)attribute.Min, (long)attribute.Max, config, valueLabel);
            }
            else
            {
                return (ushort)SirenixEditorFields.ProgressBarField(rect, label, (double)value, attribute.Min, attribute.Max, config, valueLabel);
            }
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    [OdinDrawer]
    public sealed class ProgressBarAttributeIntDrawer : BaseProgressBarAttributeDrawer<int>
    {
        /// <summary>
        /// Draws a progress bar for an int property.
        /// </summary>
        protected override int DrawProgressBar(Rect rect, GUIContent label, int value, ProgressBarAttribute attribute, ProgressBarConfig config, string valueLabel)
        {
            if (attribute.Segmented)
            {
                return (int)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)value, (long)attribute.Min, (long)attribute.Max, config, valueLabel);
            }
            else
            {
                return (int)SirenixEditorFields.ProgressBarField(rect, label, (double)value, attribute.Min, attribute.Max, config, valueLabel);
            }
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    [OdinDrawer]
    public sealed class ProgressBarAttributeUintDrawer : BaseProgressBarAttributeDrawer<uint>
    {
        /// <summary>
        /// Draws a progress bar for a uint property.
        /// </summary>
        protected override uint DrawProgressBar(Rect rect, GUIContent label, uint value, ProgressBarAttribute attribute, ProgressBarConfig config, string valueLabel)
        {
            if (attribute.Segmented)
            {
                return (uint)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)value, (long)attribute.Min, (long)attribute.Max, config, valueLabel);
            }
            else
            {
                return (uint)SirenixEditorFields.ProgressBarField(rect, label, (double)value, attribute.Min, attribute.Max, config, valueLabel);
            }
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    [OdinDrawer]
    public sealed class ProgressBarAttributeLongDrawer : BaseProgressBarAttributeDrawer<long>
    {
        /// <summary>
        /// Draws a progress bar for a long property.
        /// </summary>
        protected override long DrawProgressBar(Rect rect, GUIContent label, long value, ProgressBarAttribute attribute, ProgressBarConfig config, string valueLabel)
        {
            if (attribute.Segmented)
            {
                return (long)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)value, (long)attribute.Min, (long)attribute.Max, config, valueLabel);
            }
            else
            {
                return (long)SirenixEditorFields.ProgressBarField(rect, label, (double)value, attribute.Min, attribute.Max, config, valueLabel);
            }
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    [OdinDrawer]
    public sealed class ProgressBarAttributeUlongDrawer : BaseProgressBarAttributeDrawer<ulong>
    {
        /// <summary>
        /// Draws a progress bar for a ulong property.
        /// </summary>
        protected override ulong DrawProgressBar(Rect rect, GUIContent label, ulong value, ProgressBarAttribute attribute, ProgressBarConfig config, string valueLabel)
        {
            if (attribute.Segmented)
            {
                return (ulong)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)value, (long)attribute.Min, (long)attribute.Max, config, valueLabel);
            }
            else
            {
                return (ulong)SirenixEditorFields.ProgressBarField(rect, label, (double)value, attribute.Min, attribute.Max, config, valueLabel);
            }
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    [OdinDrawer]
    public sealed class ProgressBarAttributeFloatDrawer : BaseProgressBarAttributeDrawer<float>
    {
        /// <summary>
        /// Draws a progress bar for a float property.
        /// </summary>
        protected override float DrawProgressBar(Rect rect, GUIContent label, float value, ProgressBarAttribute attribute, ProgressBarConfig config, string valueLabel)
        {
            if (attribute.Segmented)
            {
                return (float)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)value, (long)attribute.Min, (long)attribute.Max, config, valueLabel);
            }
            else
            {
                return (float)SirenixEditorFields.ProgressBarField(rect, label, (double)value, attribute.Min, attribute.Max, config, valueLabel);
            }
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    [OdinDrawer]
    public sealed class ProgressBarAttributedoubleDrawer : BaseProgressBarAttributeDrawer<double>
    {
        /// <summary>
        /// Draws a progress bar for a double property.
        /// </summary>
        protected override double DrawProgressBar(Rect rect, GUIContent label, double value, ProgressBarAttribute attribute, ProgressBarConfig config, string valueLabel)
        {
            if (attribute.Segmented)
            {
                return (double)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)value, (long)attribute.Min, (long)attribute.Max, config, valueLabel);
            }
            else
            {
                return (double)SirenixEditorFields.ProgressBarField(rect, label, (double)value, attribute.Min, attribute.Max, config, valueLabel);
            }
        }
    }

    /// <summary>
    /// Draws values decorated with <see cref="ProgressBarAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    [OdinDrawer]
    public sealed class ProgressBarAttributedecimalDrawer : BaseProgressBarAttributeDrawer<decimal>
    {
        /// <summary>
        /// Draws a progress bar for a decimal property.
        /// </summary>
        protected override decimal DrawProgressBar(Rect rect, GUIContent label, decimal value, ProgressBarAttribute attribute, ProgressBarConfig config, string valueLabel)
        {
            if (attribute.Segmented)
            {
                return (decimal)SirenixEditorFields.SegmentedProgressBarField(rect, label, (long)value, (long)attribute.Min, (long)attribute.Max, config, valueLabel);
            }
            else
            {
                return (decimal)SirenixEditorFields.ProgressBarField(rect, label, (double)value, attribute.Min, attribute.Max, config, valueLabel);
            }
        }
    }
}
#endif