//-----------------------------------------------------------------------
// <copyright file="UInt64Serializer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Globalization;

namespace Sirenix.Serialization
{
    /// <summary>
    /// Serializer for the <see cref="ulong"/> type.
    /// </summary>
    /// <seealso cref="Sirenix.Serialization.Serializer{System.UInt64}" />
    public sealed class UInt64Serializer : Serializer<ulong>
    {
        /// <summary>
        /// Reads a value of type <see cref="ulong" />.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>
        /// The value which has been read.
        /// </returns>
        public override ulong ReadValue(IDataReader reader)
        {
            string name;
            var entry = reader.PeekEntry(out name);

            if (entry == EntryType.Integer)
            {
                ulong value;
                if (reader.ReadUInt64(out value) == false)
                {
                    reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry.ToString());
                }
                return value;
            }
            else
            {
                reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.Integer.ToString() + ", but got entry '" + name + "' of type " + entry.ToString());
                reader.SkipEntry();
                return default(ulong);
            }
        }

        /// <summary>
        /// Writes a value of type <see cref="ulong" />.
        /// </summary>
        /// <param name="name">The name of the value to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="writer">The writer to use.</param>
        public override void WriteValue(string name, ulong value, IDataWriter writer)
        {
            FireOnSerializedType();
            writer.WriteUInt64(name, value);
        }
    }
}