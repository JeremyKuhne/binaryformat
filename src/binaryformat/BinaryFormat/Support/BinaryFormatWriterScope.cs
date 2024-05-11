﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BinaryFormat.Records;
using System.Text;

namespace BinaryFormat;

internal readonly ref struct BinaryFormatWriterScope
{
    internal BinaryWriter Writer { get; }

    public BinaryFormatWriterScope(Stream stream)
    {
        Writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        SerializationHeader.Default.Write(Writer);
    }

    public static implicit operator BinaryWriter(in BinaryFormatWriterScope scope) => scope.Writer;

    public void Dispose()
    {
        MessageEnd.Instance.Write(Writer);
        Writer.Dispose();
    }
}
