// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FormatTests.Common;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;

namespace FormatTests.FormattedObject;

public class FormattedObjectSerializer : ISerializer
{
    public static object Deserialize(
        Stream stream,
        SerializationBinder? binder = null,
        FormatterAssemblyStyle assemblyMatching = FormatterAssemblyStyle.Simple,
        ISurrogateSelector? surrogateSelector = null)
    {
        BinaryFormattedObject.Options options = new()
        {
            Binder = binder,
            SurrogateSelector = surrogateSelector,
        };

        if (assemblyMatching != FormatterAssemblyStyle.Simple)
        {
            options.Flags = BinaryFormattedObject.OptionFlags.MatchFullAssemblyNames | BinaryFormattedObject.OptionFlags.RequireFieldsHaveData;
        }

        BinaryFormattedObject format = new(stream, options);
        return format.Deserialize();
    }
}
