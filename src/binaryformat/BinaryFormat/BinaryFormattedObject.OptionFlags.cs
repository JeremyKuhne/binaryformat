// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Serialization;

namespace BinaryFormat;

public sealed partial class BinaryFormattedObject
{
    [Flags]
    public enum OptionFlags
    {
        /// <summary>
        ///  How exactly assembly names need to match for deserialization.
        /// </summary>
        MatchFullAssemblyNames              = 0b0000_0000_0000_0001,

        /// <summary>
        ///  Require fields have data, even if they have <see cref="OptionalFieldAttribute"/>.
        /// </summary>
        RequireFieldsHaveData               = 0b0000_0000_0000_0010
    }
}
