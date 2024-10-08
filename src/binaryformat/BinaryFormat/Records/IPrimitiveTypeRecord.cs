﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BinaryFormat.Records;

/// <summary>
///  Record that represents a primitive type or an array of primitive types.
/// </summary>
public interface IPrimitiveTypeRecord : IRecord
{
    PrimitiveType PrimitiveType { get; }
}
