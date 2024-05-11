// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace BinaryFormat.Records;

/// <summary>
///  Single dimensional array of strings.
/// </summary>
/// <remarks>
///  <para>
///   <see href="https://learn.microsoft.com/openspecs/windows_protocols/ms-nrbf/3d98fd60-d2b4-448a-ac0b-3cd8dea41f9d">
///    [MS-NRBF] 2.4.3.4
///   </see>
///  </para>
/// </remarks>
public sealed class ArraySingleString : ArrayRecord<string?>, IRecord<ArraySingleString>, IBinaryFormatParseable<ArraySingleString>
{
    public static RecordType RecordType => RecordType.ArraySingleString;

    private readonly IReadOnlyList<object?> _records;

    internal ArraySingleString(Id objectId, IReadOnlyList<object?> arrayObjects, IReadOnlyRecordMap recordMap)
        : base(new ArrayInfo(objectId, arrayObjects.Count), new StringListAdapter(arrayObjects, recordMap))
    {
        _records = arrayObjects;
    }

    static ArraySingleString IBinaryFormatParseable<ArraySingleString>.Parse(BinaryFormattedObject.IParseState state) => new(
        ArrayInfo.Parse(state.Reader, out Count length),
        ReadObjectArrayValues(state, length), state.RecordMap);

    private protected override void Write(BinaryWriter writer)
    {
        writer.Write((byte)RecordType);
        _arrayInfo.Write(writer);
        WriteRecords(writer, _records, coalesceNulls: true);
    }

    private sealed class StringListAdapter : IReadOnlyList<string?>
    {
        private readonly IReadOnlyList<object?> _recordList;
        private readonly IReadOnlyRecordMap _recordMap;

        internal StringListAdapter(IReadOnlyList<object?> recordList, IReadOnlyRecordMap recordMap)
        {
            _recordList = recordList;
            _recordMap = recordMap;
        }

        public string? this[int index] => _recordList[index] switch
        {
            null => null,
            IRecord record => _recordMap.Dereference(record) is BinaryObjectString stringRecord
                ? stringRecord.Value
                : throw new InvalidOperationException(),
            _ => throw new InvalidOperationException()
        };

        int IReadOnlyCollection<string?>.Count => _recordList.Count;

        public IEnumerator<string?> GetEnumerator()
        {
            return GetEnumerable().GetEnumerator();

            IEnumerable<string?> GetEnumerable()
            {
                for (int i = 0; i < _recordList.Count; i++)
                {
                    yield return this[i];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
