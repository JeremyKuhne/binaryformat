﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace FormatTests.FormattedObject;

public class MemberTypeInfoTests
{
    private static readonly byte[] s_hashtableMemberInfo =
    [
        0x00, 0x00, 0x03, 0x03, 0x00, 0x05, 0x05, 0x0b, 0x08, 0x1c, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d,
        0x2e, 0x43, 0x6f, 0x6c, 0x6c, 0x65, 0x63, 0x74, 0x69, 0x6f, 0x6e, 0x73, 0x2e, 0x49, 0x43, 0x6f,
        0x6d, 0x70, 0x61, 0x72, 0x65, 0x72, 0x24, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e, 0x43, 0x6f,
        0x6c, 0x6c, 0x65, 0x63, 0x74, 0x69, 0x6f, 0x6e, 0x73, 0x2e, 0x49, 0x48, 0x61, 0x73, 0x68, 0x43,
        0x6f, 0x64, 0x65, 0x50, 0x72, 0x6f, 0x76, 0x69, 0x64, 0x65, 0x72, 0x08
    ];

    [Fact]
    public void MemberTypeInfo_ReadHashtable()
    {
        using BinaryReader reader = new(new MemoryStream(s_hashtableMemberInfo));
        IReadOnlyList<MemberTypeInfo> info = MemberTypeInfo.Parse(reader, 7);

        info.Should().BeEquivalentTo(new MemberTypeInfo[]
        {
            new(BinaryType.Primitive, PrimitiveType.Single),
            new(BinaryType.Primitive, PrimitiveType.Int32),
            new(BinaryType.SystemClass, "System.Collections.IComparer"),
            new(BinaryType.SystemClass, "System.Collections.IHashCodeProvider"),
            new(BinaryType.Primitive, PrimitiveType.Int32),
            new(BinaryType.ObjectArray, null),
            new(BinaryType.ObjectArray, null)
        });
    }

    [Fact]
    public void MemberTypeInfo_HashtableRoundTrip()
    {
        using BinaryReader reader = new(new MemoryStream(s_hashtableMemberInfo));
        IReadOnlyList<MemberTypeInfo> info = MemberTypeInfo.Parse(reader, 7);

        MemoryStream stream = new();
        BinaryWriter writer = new(stream);
        writer.Write(info);
        stream.Position = 0;

        using BinaryReader reader2 = new(stream);
        info = MemberTypeInfo.Parse(reader2, 7);
        info.Should().BeEquivalentTo(new MemberTypeInfo[]
        {
            new(BinaryType.Primitive, PrimitiveType.Single),
            new(BinaryType.Primitive, PrimitiveType.Int32),
            new(BinaryType.SystemClass, "System.Collections.IComparer"),
            new(BinaryType.SystemClass, "System.Collections.IHashCodeProvider"),
            new(BinaryType.Primitive, PrimitiveType.Int32),
            new(BinaryType.ObjectArray, null),
            new(BinaryType.ObjectArray, null)
        });
    }

    [Fact]
    public void MemberTypeInfo_ReadHashtable_TooShort()
    {
        MemoryStream stream = new(s_hashtableMemberInfo);
        stream.SetLength(stream.Length - 1);
        using BinaryReader reader = new(stream);
        Action action = () => MemberTypeInfo.Parse(reader, 7);
        action.Should().Throw<EndOfStreamException>();
    }
}
