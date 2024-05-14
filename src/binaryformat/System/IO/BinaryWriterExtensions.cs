// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BinaryFormat.Records;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.IO;

internal static class BinaryWriterExtensions
{
    /// <summary>
    ///  Writes a <see cref="DateTime"/> object to the given <paramref name="writer"/>.
    /// </summary>
    public static void Write(this BinaryWriter writer, DateTime value)
    {
        // Copied from System.Runtime.Serialization.Formatters.Binary.BinaryFormatterWriter

        // In .NET Framework, BinaryFormatter is able to access DateTime's ToBinaryRaw,
        // which just returns the value of its sole Int64 dateData field.  Here, we don't
        // have access to that member (which doesn't even exist anymore, since it was only for
        // BinaryFormatter, which is now in a separate assembly).  To address that,
        // we access the sole field directly via an unsafe cast.
        long dateData = Unsafe.As<DateTime, long>(ref value);
        writer.Write(dateData);
    }

    /// <summary>
    ///  Writes a collection of primitives.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Only supports <see langword="bool"/>, <see langword="byte"/>, <see langword="sbyte"/>, <see langword="char"/>,
    ///   <see langword="short"/>, <see langword="ushort"/>, <see langword="int"/>, <see langword="uint"/>,
    ///   <see langword="long"/>, <see langword="ulong"/>, <see langword="float"/>, <see langword="double"/>,
    ///   <see langword="decimal"/>, <see cref="DateTime"/>, and <see cref="TimeSpan"/>.
    ///  </para>
    /// </remarks>
    internal static unsafe void WritePrimitives<T>(this BinaryWriter writer, IReadOnlyList<T> values)
        where T : unmanaged
    {
        if (values.Count == 0)
        {
            return;
        }

        if (typeof(T) == typeof(DateTime)
            || typeof(T) == typeof(decimal)
            || typeof(T) == typeof(TimeSpan))
        {
            WritePrimitiveCollection(writer, values);
            return;
        }

        if (typeof(T) != typeof(bool)
            && typeof(T) != typeof(byte)
            && typeof(T) != typeof(sbyte)
            && typeof(T) != typeof(char)
            && typeof(T) != typeof(short)
            && typeof(T) != typeof(ushort)
            && typeof(T) != typeof(int)
            && typeof(T) != typeof(uint)
            && typeof(T) != typeof(long)
            && typeof(T) != typeof(ulong)
            && typeof(T) != typeof(float)
            && typeof(T) != typeof(double))
        {
            throw new ArgumentException($"Cannot write primitives of {typeof(T).Name}.", nameof(T));
        }

        ReadOnlySpan<T> span;
        if (values is T[] array)
        {
            span = array;
        }
        else if (values is ArraySegment<T> arraySegment)
        {
            span = arraySegment;
        }
        else if (values is List<T> list)
        {
            span = CollectionsMarshal.AsSpan(list);
        }
        else
        {
            WritePrimitiveCollection(writer, values);
            return;
        }

        if (typeof(T) == typeof(char))
        {
            // Need to handle different encodings
            // (Is there a more efficient way to do the cast?)
            writer.Write(MemoryMarshal.Cast<T, char>(span));
            return;
        }

        if (sizeof(T) == 1 || BitConverter.IsLittleEndian)
        {
            writer.Write(MemoryMarshal.Cast<T, byte>(span));
            return;
        }

        // This could potentially be optimized by writing all of the data to a temporary buffer and reversing the
        // endianness in one pass with BinaryPrimitives.ReverseEndianness (see ReadPrimitiveArray). Probably not
        // worth doing without measuring to see how much of a difference it actualy makes.
        WritePrimitiveCollection(writer, values);

        static void WritePrimitiveCollection(BinaryWriter writer, IReadOnlyList<T> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (typeof(T) == typeof(bool))
                {
                    writer.Write((bool)(object)values[i]);
                }
                else if (typeof(T) == typeof(byte))
                {
                    writer.Write((byte)(object)values[i]);
                }
                else if (typeof(T) == typeof(sbyte))
                {
                    writer.Write((sbyte)(object)values[i]);
                }
                else if (typeof(T) == typeof(char))
                {
                    writer.Write((char)(object)values[i]);
                }
                else if (typeof(T) == typeof(short))
                {
                    writer.Write((short)(object)values[i]);
                }
                else if (typeof(T) == typeof(ushort))
                {
                    writer.Write((ushort)(object)values[i]);
                }
                else if (typeof(T) == typeof(int))
                {
                    writer.Write((int)(object)values[i]);
                }
                else if (typeof(T) == typeof(uint))
                {
                    writer.Write((uint)(object)values[i]);
                }
                else if (typeof(T) == typeof(long))
                {
                    writer.Write((long)(object)values[i]);
                }
                else if (typeof(T) == typeof(ulong))
                {
                    writer.Write((ulong)(object)values[i]);
                }
                else if (typeof(T) == typeof(float))
                {
                    writer.Write((float)(object)values[i]);
                }
                else if (typeof(T) == typeof(double))
                {
                    writer.Write((double)(object)values[i]);
                }
                else if (typeof(T) == typeof(decimal))
                {
                    writer.Write(((decimal)(object)values[i]).ToString(CultureInfo.InvariantCulture));
                }
                else if (typeof(T) == typeof(DateTime))
                {
                    writer.Write((DateTime)(object)values[i]);
                }
                else if (typeof(T) == typeof(TimeSpan))
                {
                    writer.Write(((TimeSpan)(object)values[i]).Ticks);
                }
                else
                {
                    throw new SerializationException($"Failure trying to write primitive '{typeof(T)}'");
                }
            }
        }
    }

    /// <summary>
    ///  Writes <see cref="MemberTypeInfo"/>.
    /// </summary>
    internal static void Write(this BinaryWriter writer, IReadOnlyList<MemberTypeInfo> memberTypeInfo)
    {
        for (int i = 0; i < memberTypeInfo.Count; i++)
        {
            writer.Write((byte)memberTypeInfo[i].Type);
        }

        for (int i = 0; i < memberTypeInfo.Count; i++)
        {
            switch (memberTypeInfo[i].Type)
            {
                case BinaryType.Primitive:
                case BinaryType.PrimitiveArray:
                    writer.Write((byte)memberTypeInfo[i].Info!);
                    break;
                case BinaryType.SystemClass:
                    writer.Write((string)memberTypeInfo[i].Info!);
                    break;
                case BinaryType.Class:
                    ((ClassTypeInfo)memberTypeInfo[i].Info!).Write(writer);
                    break;
                case BinaryType.String:
                case BinaryType.ObjectArray:
                case BinaryType.StringArray:
                case BinaryType.Object:
                    // Other types have no additional data.
                    break;
                default:
                    throw new SerializationException("Unexpected binary type.");
            }
        }
    }
}
