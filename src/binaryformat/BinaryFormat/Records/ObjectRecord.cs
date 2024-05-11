﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BinaryFormat.Records;

public abstract class ObjectRecord : Record
{
    public abstract Id ObjectId { get; }

    private protected override Id Id => ObjectId;
}
