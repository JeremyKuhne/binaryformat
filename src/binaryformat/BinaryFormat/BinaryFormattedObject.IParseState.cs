﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BinaryFormat.Records;

namespace BinaryFormat;

public sealed partial class BinaryFormattedObject
{
    /// <summary>
    ///  Parsing state.
    /// </summary>
    internal interface IParseState
    {
        BinaryReader Reader { get; }
        RecordMap RecordMap { get; }
        Options Options { get; }
        ITypeResolver TypeResolver { get; }
    }
}
