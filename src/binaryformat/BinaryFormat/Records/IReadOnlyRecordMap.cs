// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BinaryFormat.Records;

/// <summary>
///  Map of records.
/// </summary>
public interface IReadOnlyRecordMap
{
    /// <summary>
    ///  Gets a record by its identifier.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Not all records have identifiers, only ones that can be referenced by other records.
    ///  </para>
    /// </remarks>
    IRecord this[Id id] { get; }
}
