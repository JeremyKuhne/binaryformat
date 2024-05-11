﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using System.Runtime.Serialization;

namespace BinaryFormat.Deserializer;

#pragma warning disable SYSLIB0050 // Type or member is obsolete

internal sealed class PendingSerializationInfo
{
    internal int ObjectId { get; }
    private readonly ISerializationSurrogate? _surrogate;
    private readonly SerializationInfo _info;

    internal PendingSerializationInfo(
        int objectId,
        SerializationInfo info,
        ISerializationSurrogate? surrogate)
    {
        ObjectId = objectId;
        _surrogate = surrogate;
        _info = info;
    }

    [UnconditionalSuppressMessage(
        "ReflectionAnalysis",
        "IL2072:UnrecognizedReflectionPattern",
        Justification = "Not sure how to attribute this.")]
    internal void Populate(IDictionary<int, object> objects, StreamingContext context)
    {
        object @object = objects[ObjectId];
        Type type = @object.GetType();

        if (_surrogate is not null)
        {
            object populated = _surrogate.SetObjectData(@object, _info, context, selector: null);
            if (populated is null)
            {
                // Sort of odd to allow returning null to ignore setting the returned value back,
                // but that is the way this worked in the BinaryFormatter.
                return;
            }

            // Don't use == on reference types as we are dependent on the instance in the objects
            // dictionary never changing. Value types can be modified but this is ok as any usages
            // of this object will have a pending fixup that will reapply the value.
            //
            // BinaryFormatter would allow this to change as long as you didn't wrap the surrogate
            // in FormaterServices.GetSurrogateForCyclicalReference. We break here on value types
            // as they can never be observed in an unfinished state.
            if (!type.IsValueType && !ReferenceEquals(populated, @object))
            {
                throw new SerializationException("Surrogate must return the same object that was provided in the 'obj' parameter.");
            }

            objects[ObjectId] = populated;
            return;
        }

        ConstructorInfo constructor = GetDeserializationConstructor(type);
        constructor.Invoke(@object, [_info, context]);
    }

    private static ConstructorInfo GetDeserializationConstructor(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type type)
    {
        foreach (ConstructorInfo constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            ParameterInfo[] parameters = constructor.GetParameters();
            if (parameters.Length == 2
                && parameters[0].ParameterType == typeof(SerializationInfo)
                && parameters[1].ParameterType == typeof(StreamingContext))
            {
                return constructor;
            }
        }

        throw new SerializationException($"The constructor to deserialize an object of type '{type.FullName}' was not found.");
    }
}

#pragma warning restore SYSLIB0050 // Type or member is obsolete
