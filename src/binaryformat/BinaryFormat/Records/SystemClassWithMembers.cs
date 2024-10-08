﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BinaryFormat.Records;

/// <summary>
///  System class information.
/// </summary>
/// <remarks>
///  <para>
///   <see href="https://learn.microsoft.com/openspecs/windows_protocols/ms-nrbf/f5bd730f-d944-42ab-b6b3-013099559a4b">
///    [MS-NRBF] 2.3.2.4
///   </see>
///  </para>
/// </remarks>
internal sealed class SystemClassWithMembers : ClassRecord, IRecord<SystemClassWithMembers>, IBinaryFormatParseable<SystemClassWithMembers>
{
    private SystemClassWithMembers(ClassInfo classInfo, IReadOnlyList<MemberTypeInfo> memberTypeInfo, IReadOnlyList<object?> memberValues)
        : base(classInfo, memberTypeInfo, memberValues) { }

    public static RecordType RecordType => RecordType.SystemClassWithMembers;

    static SystemClassWithMembers IBinaryFormatParseable<SystemClassWithMembers>.Parse(
        BinaryFormattedObject.IParseState state)
    {
        ClassInfo classInfo = ClassInfo.Parse(state.Reader, out _);
        IReadOnlyList<MemberTypeInfo> memberTypeInfo = Records.MemberTypeInfo.CreateFromClassInfoAndLibrary(state, classInfo, Id.Null);
        return new(
            classInfo,
            memberTypeInfo,
            ReadObjectMemberValues(state, memberTypeInfo));
    }

    private protected override void Write(BinaryWriter writer)
    {
        // Really shouldn't be writing this record type. It isn't as safe as the typed variant
        // and saves very little space.
        throw new NotSupportedException();
    }
}
