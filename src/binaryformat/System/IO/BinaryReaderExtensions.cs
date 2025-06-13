// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers.Binary;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.IO;

internal static class BinaryReaderExtensions
{
    /// <summary>
    ///  Reads a binary formatted <see cref="DateTime"/> from the given <paramref name="reader"/>.
    /// </summary>
    /// <exception cref="SerializationException">The data was invalid.</exception>
    internal static unsafe DateTime ReadDateTime(this BinaryReader reader)
        => CreateDateTimeFromData(reader.ReadInt64());

    /// <summary>
    ///  Creates a <see cref="DateTime"/> object from raw data with validation.
    /// </summary>
    /// <exception cref="SerializationException"><paramref name="data"/> was invalid.</exception>
    internal static DateTime CreateDateTimeFromData(long data)
    {
        // Copied from System.Runtime.Serialization.Formatters.Binary.BinaryParser

        // Use DateTime's public constructor to validate the input, but we
        // can't return that result as it strips off the kind. To address
        // that, store the value directly into a DateTime via an unsafe cast.
        // See BinaryFormatterWriter.WriteDateTime for details.

        try
        {
            const long TicksMask = 0x3FFFFFFFFFFFFFFF;
            _ = new DateTime(data & TicksMask);
        }
        catch (ArgumentException ex)
        {
            // Bad data
            throw new SerializationException(ex.Message, ex);
        }

        return Unsafe.As<long, DateTime>(ref data);
    }

    /// <summary>
    ///  Returns the remaining amount of bytes in the given <paramref name="reader"/>.
    /// </summary>
    internal static long Remaining(this BinaryReader reader)
    {
        Stream stream = reader.BaseStream;
        return stream.Length - stream.Position;
    }

    /// <summary>
    ///  Reads an array of primitives.
    /// </summary>
    /// <inheritdoc cref="BinaryWriterExtensions.WritePrimitives{T}(BinaryWriter, IReadOnlyList{T})"/>
    internal static unsafe T[] ReadPrimitiveArray<T>(this BinaryReader reader, int count)
        where T : unmanaged
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if (typeof(T) == typeof(decimal) || typeof(T) == typeof(DateTime) || typeof(T) == typeof(TimeSpan))
        {
            if (count == 0)
            {
                return [];
            }

            // Decimal is persisted as a UTF-8 string. It has a 7-bit encoded length so it could be, in theory just
            // a few bytes. Picking 2 bytes- once the minimum string length (and termination if applicable) are
            // evaluated, this could be made more aggressive. DateTime and TimeSpan match their stored sizes.
            //
            // Note that we also have a hard cap on the initial collection size in these cases.
            if (count > 0 && reader.Remaining() < checked(count * (typeof(T) == typeof(decimal) ? 2 : sizeof(T))))
            {
                throw new SerializationException("Not enough data to fill array.");
            }

            return ReadNonBlittableTypes(reader, count);
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
            throw new ArgumentException($"Cannot read primitives of {typeof(T).Name}.", nameof(T));
        }

        if (count > 0 && reader.Remaining() < checked(count * (typeof(T) == typeof(char) ? 1 : sizeof(T))))
        {
            throw new SerializationException("Not enough data to fill array.");
        }

        if (count == 0)
        {
            return [];
        }

        if (typeof(T) == typeof(char))
        {
            // Need to handle different encodings
            return (T[])(object)reader.ReadChars(count);
        }

        T[] array = GC.AllocateUninitializedArray<T>(count);

        fixed (T* a = array)
        {
            Span<byte> arrayData = new(a, array.Length * sizeof(T));

            if (reader.Read(arrayData) != arrayData.Length)
            {
                throw new SerializationException("Not enough data to fill array.");
            }

            if (sizeof(T) != 1 && !BitConverter.IsLittleEndian)
            {
                if (sizeof(T) == 2)
                {
                    Span<ushort> ushorts = MemoryMarshal.Cast<byte, ushort>(arrayData);
                    BinaryPrimitives.ReverseEndianness(ushorts, ushorts);
                }
                else if (sizeof(T) == 4)
                {
                    Span<int> ints = MemoryMarshal.Cast<byte, int>(arrayData);
                    BinaryPrimitives.ReverseEndianness(ints, ints);
                }
                else if (sizeof(T) == 8)
                {
                    Span<long> longs = MemoryMarshal.Cast<byte, long>(arrayData);
                    BinaryPrimitives.ReverseEndianness(longs, longs);
                }
                else
                {
                    throw new InvalidOperationException($"Cannot read primitives of {typeof(T).Name}.");
                }
            }
        }

        return array;

        static T[] ReadNonBlittableTypes(BinaryReader reader, int count)
        {
            // We've already made one check for remaining data. Decimal is a weird case as it is 16 bytes and is
            // persisted as a UTF-8 string. Out of an abundance of caution, we'll max out what we pre-allocate to avoid
            // untrusted data claiming a huge number of decimal strings. Worst case is that roughly 4x what the remaining
            // data could contain at the smallest string size, but we'll still guard.

            T[] values = GC.AllocateUninitializedArray<T>(count);

            for (int i = 0; i < count; i++)
            {
                if (typeof(T) == typeof(decimal))
                {
                    values[i] = (T)(object)decimal.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
                }
                else if (typeof(T) == typeof(DateTime))
                {
                    values[i] = (T)(object)reader.ReadDateTime();
                }
                else if (typeof(T) == typeof(TimeSpan))
                {
                    values[i] = (T)(object)new TimeSpan(reader.ReadInt64());
                }
                else
                {
                    throw new SerializationException($"Invalid primitive type '{typeof(T)}'");
                }
            }

            return values;
        }
    }
}
