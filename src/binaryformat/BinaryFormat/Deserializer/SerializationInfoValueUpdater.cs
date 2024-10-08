﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Serialization;

namespace BinaryFormat.Deserializer;

internal sealed class SerializationInfoValueUpdater : ValueUpdater
{
    private readonly SerializationInfo _info;
    private readonly string _name;

    internal SerializationInfoValueUpdater(int objectId, int valueId, SerializationInfo info, string name) : base(objectId, valueId)
    {
        _info = info;
        _name = name;
    }

    internal override void UpdateValue(IDictionary<int, object> objects)
    {
        object newValue = objects[ValueId];
        _info.UpdateValue(_name, newValue, newValue.GetType());
    }
}
