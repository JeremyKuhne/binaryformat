// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BinaryFormat.Records;

/// <summary>
///  <see cref="BinaryArray"/> of objects.
/// </summary>
/// <inheritdoc cref="BinaryArray"/>
public sealed class BinaryArrayObject : ArrayRecord<object?>, IRecord<BinaryArrayObject>, IBinaryArray
{
    public Count Rank { get; }
    public BinaryArrayType ArrayType { get; }
    public MemberTypeInfo TypeInfo { get; }
    public IReadOnlyList<int> Lengths { get; }

    internal BinaryArrayObject(
        Count rank,
        BinaryArrayType type,
        IReadOnlyList<int> lengths,
        ArrayInfo arrayInfo,
        MemberTypeInfo typeInfo,
        BinaryFormattedObject.IParseState state)
        : base(arrayInfo, ReadObjectArrayValues(state, typeInfo[0].Type, typeInfo[0].Info, arrayInfo.Length))
    {
        Rank = rank;
        ArrayType = type;
        TypeInfo = typeInfo;
        Lengths = lengths;
    }

    private protected override void Write(BinaryWriter writer) => throw new NotSupportedException();
}
