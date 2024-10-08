﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BinaryFormat.Records;

/// <summary>
///  Library full name information.
/// </summary>
/// <remarks>
///  <para>
///   <see href="https://learn.microsoft.com/openspecs/windows_protocols/ms-nrbf/7fcf30e1-4ad4-4410-8f1a-901a4a1ea832">
///    [MS-NRBF] 2.6.2
///   </see>
///  </para>
/// </remarks>
public sealed class BinaryLibrary : IRecord<BinaryLibrary>, IBinaryFormatParseable<BinaryLibrary>
{
    public Id LibraryId { get; }
    public string LibraryName { get; }
    Id IRecord.Id => LibraryId;

    internal BinaryLibrary(Id libraryId, string libraryName)
    {
        LibraryId = libraryId;
        LibraryName = libraryName;
    }

    public static RecordType RecordType => RecordType.BinaryLibrary;

    static BinaryLibrary IBinaryFormatParseable<BinaryLibrary>.Parse(BinaryFormattedObject.IParseState state) =>
        new(state.Reader.ReadInt32(), state.Reader.ReadString());

    internal void Write(BinaryWriter writer)
    {
        writer.Write((byte)RecordType);
        writer.Write(LibraryId);
        writer.Write(LibraryName);
    }
}
