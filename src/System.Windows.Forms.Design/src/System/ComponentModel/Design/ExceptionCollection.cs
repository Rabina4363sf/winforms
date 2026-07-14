// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Serialization;

namespace System.ComponentModel.Design;

public sealed class ExceptionCollection : Exception
{
    private readonly List<Exception>? _exceptions;

    public ExceptionCollection(List<Exception>? exceptions)
    {
        if (exceptions is null)
        {
            return;
        }

        _exceptions = exceptions is null ? null : new List<Exception>(exceptions);
    }

    public IReadOnlyList<Exception>? Exceptions => _exceptions;

    [Obsolete(DiagnosticId = "SYSLIB0051")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        throw new PlatformNotSupportedException();
    }
}
