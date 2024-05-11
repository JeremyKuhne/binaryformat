// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BinaryFormat.Records;

/// <summary>
///  <see cref="BinaryArray"/> of primitive values.
/// </summary>
/// <inheritdoc cref="BinaryArray"/>
public sealed class BinaryArrayPrimitive<T> : ArrayRecord<T>, IRecord<BinaryArrayPrimitive<T>>, IBinaryArray, IPrimitiveTypeRecord where T : unmanaged
{
    public Count Rank { get; }
    public BinaryArrayType ArrayType { get; }
    public MemberTypeInfo TypeInfo { get; }
    public IReadOnlyList<int> Lengths { get; }
    public PrimitiveType PrimitiveType { get; }

    internal BinaryArrayPrimitive(
        Count rank,
        BinaryArrayType arrayType,
        IReadOnlyList<int> lengths,
        ArrayInfo arrayInfo,
        MemberTypeInfo typeInfo,
        BinaryReader reader)
        : base(arrayInfo, reader.ReadPrimitiveArray<T>(arrayInfo.Length))
    {
        Rank = rank;
        ArrayType = arrayType;
        TypeInfo = typeInfo;
        Lengths = lengths;
        PrimitiveType = (PrimitiveType)typeInfo[0].Info!;
    }

    private protected override void Write(BinaryWriter writer) => throw new NotSupportedException();
}
