// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BinaryFormat.Records;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Text;

namespace BinaryFormat;

/// <summary>
///  Object model for the binary format (NRBF) put out by BinaryFormatter. It parses and creates a model but does not
///  instantiate any reference types outside of <see langword="string"/> and arrays of <see cref="PrimitiveType"/>.
/// </summary>
/// <remarks>
///  <para>
///   This is useful for explicitly controlling the rehydration of binary formatted data.
///  </para>
/// </remarks>
public sealed partial class BinaryFormattedObject
{
#pragma warning disable SYSLIB0050 // Type or member is obsolete
    internal static FormatterConverter DefaultConverter { get; } = new();
#pragma warning restore SYSLIB0050

    private static readonly Options s_defaultOptions = new();
    private readonly Options _options;

    private readonly RecordMap _recordMap = new();

    private ITypeResolver? _typeResolver;
    private ITypeResolver TypeResolver => _typeResolver ??= new DefaultTypeResolver(_options, _recordMap);

    private readonly Id _rootRecord;

    /// <summary>
    ///  Creates <see cref="BinaryFormattedObject"/> by parsing <paramref name="stream"/>.
    /// </summary>
    public BinaryFormattedObject(Stream stream, Options? options = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using BinaryReader reader = new(stream, Encoding.UTF8, leaveOpen: true);

        _options = options ?? s_defaultOptions;

        ParseState state = new(reader, this);

        IRecord? currentRecord;

        try
        {
            currentRecord = Record.ReadBinaryFormatRecord(state);
            if (currentRecord is not SerializationHeader header)
            {
                throw new SerializationException("Did not find serialization header.");
            }

            _rootRecord = header.RootId;

            do
            {
                currentRecord = Record.ReadBinaryFormatRecord(state);
            }
            while (currentRecord is not MessageEnd);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidCastException or ArithmeticException or IOException)
        {
            // Make the exception easier to catch, but retain the original stack trace.
            throw ex.ConvertToSerializationException();
        }
        catch (TargetInvocationException ex)
        {
            throw ExceptionDispatchInfo.Capture(ex.InnerException!).SourceException.ConvertToSerializationException();
        }
    }

    /// <summary>
    ///  Deserializes the <see cref="BinaryFormattedObject"/> back to an object.
    /// </summary>
    [RequiresUnreferencedCode("Ultimately calls Assembly.GetType for type names in the data.")]
    public object Deserialize()
    {
        try
        {
            return Deserializer.Deserializer.Deserialize(RootRecord.Id, _recordMap, TypeResolver, _options);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidCastException or ArithmeticException or IOException)
        {
            // Make the exception easier to catch, but retain the original stack trace.
            throw ex.ConvertToSerializationException();
        }
        catch (TargetInvocationException ex)
        {
            throw ExceptionDispatchInfo.Capture(ex.InnerException!).SourceException.ConvertToSerializationException();
        }
    }

    /// <summary>
    ///  The root record of the object graph.
    /// </summary>
    public IRecord RootRecord => _recordMap[_rootRecord];

    /// <summary>
    ///  Gets a record by its identifier.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Not all records have identifiers, only ones that can be referenced by other records.
    ///  </para>
    /// </remarks>
    public IRecord this[Id id] => _recordMap[id];

    /// <summary>
    ///  Map of all records with identifiers.
    /// </summary>
    public IReadOnlyRecordMap RecordMap => _recordMap;
}
