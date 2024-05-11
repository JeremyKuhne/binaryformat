// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace FormatTests.Legacy;

public static class BinaryFormatterHelpers
{
    internal static T Clone<T>(T obj,
        ISerializationSurrogate? surrogate = null,
        FormatterAssemblyStyle assemblyFormat = FormatterAssemblyStyle.Full,
        FormatterTypeStyle typeFormat = FormatterTypeStyle.TypesAlways)
    {
        ArgumentNullException.ThrowIfNull(obj);

        BinaryFormatter f;
        if (surrogate is null)
        {
            f = new BinaryFormatter();
        }
        else
        {
            var c = new StreamingContext();
            var s = new SurrogateSelector();
            s.AddSurrogate(obj.GetType(), c, surrogate);
            f = new BinaryFormatter(s, c);
        }

        f.AssemblyFormat = assemblyFormat;
        f.TypeFormat = typeFormat;

        using (var s = new MemoryStream())
        {
            f.Serialize(s, obj);
            Assert.NotEqual(0, s.Position);
            s.Position = 0;
            return (T)f.Deserialize(s);
        }
    }

    public static byte[] ToByteArray(
        object obj,
        FormatterAssemblyStyle assemblyStyle = FormatterAssemblyStyle.Full)
    {
        BinaryFormatter bf = new()
        {
            AssemblyFormat = assemblyStyle
        };

        using MemoryStream ms = new();
        bf.Serialize(ms, obj);
        return ms.ToArray();
    }

    public static string ToBase64String(
        object obj,
        FormatterAssemblyStyle assemblyStyle = FormatterAssemblyStyle.Full)
    {
        byte[] raw = ToByteArray(obj, assemblyStyle);
        return Convert.ToBase64String(raw);
    }

    public static object FromByteArray(byte[] raw,
        FormatterAssemblyStyle assemblyStyle = FormatterAssemblyStyle.Full)
    {
        BinaryFormatter binaryFormatter = new()
        {
            AssemblyFormat = assemblyStyle
        };

        using var serializedStream = new MemoryStream(raw);
        return binaryFormatter.Deserialize(serializedStream);
    }

    public static object FromBase64String(string base64Str,
        FormatterAssemblyStyle assemblyStyle = FormatterAssemblyStyle.Full)
    {
        byte[] raw = Convert.FromBase64String(base64Str);
        return FromByteArray(raw, assemblyStyle);
    }
}
