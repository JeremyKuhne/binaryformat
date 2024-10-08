﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BinaryFormat.Records;

/// <summary>
///  Array information structure.
/// </summary>
/// <remarks>
///  <para>
///   <see href="https://learn.microsoft.com/openspecs/windows_protocols/ms-nrbf/8fac763f-e46d-43a1-b360-80eb83d2c5fb">
///    [MS-NRBF] 2.4.2.1
///   </see>
///  </para>
/// </remarks>
internal readonly struct ArrayInfo
{
    internal Id ObjectId { get; }
    internal Count Length { get; }

    internal ArrayInfo(Id objectId, Count length)
    {
        Length = length;
        ObjectId = objectId;
    }

    internal static Id Parse(BinaryReader reader, out Count length)
    {
        Id id = reader.ReadInt32();
        length = reader.ReadInt32();
        return id;
    }

    internal readonly void Write(BinaryWriter writer)
    {
        writer.Write(ObjectId);
        writer.Write(Length);
    }
}
