// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BinaryFormat.Records;

/// <summary>
///  Identifies a class by it's name and library id.
/// </summary>
/// <remarks>
///  <para>
///   <see href="https://learn.microsoft.com/openspecs/windows_protocols/ms-nrbf/844b24dd-9f82-426e-9b98-05334307a239">
///    [MS-NRBF] 2.1.1.8
///   </see>
///  </para>
/// </remarks>
internal readonly struct ClassTypeInfo
{
    public readonly string TypeName;
    public readonly Id LibraryId;

    internal ClassTypeInfo(string typeName, Id libraryId)
    {
        TypeName = typeName;
        LibraryId = libraryId;
    }

    internal static ClassTypeInfo Parse(BinaryReader reader) => new(
        reader.ReadString(),
        reader.ReadInt32());

    internal void Write(BinaryWriter writer)
    {
        writer.Write(TypeName);
        writer.Write(LibraryId);
    }
}
