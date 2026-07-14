// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.ComponentModel.Design.Tests;

public class ExceptionCollectionTests
{
    public static IEnumerable<object[]> Ctor_List_TestData()
    {
        yield return new object[] { null };
        yield return new object[] { new List<Exception>() };
        yield return new object[] { new List<Exception> { new InvalidOperationException(), new InvalidOperationException(), new InvalidOperationException() } };
    }

    [Theory]
    [MemberData(nameof(Ctor_List_TestData))]
    public void ExceptionCollection_Ctor_List(List<Exception> exceptions)
    {
        ExceptionCollection collection = new(exceptions);
        if (exceptions is null)
        {
            Assert.Null(collection.Exceptions);
        }
        else
        {
            Assert.Equal(exceptions, collection.Exceptions);
            Assert.NotSame(exceptions, collection.Exceptions);
            Assert.Equal(collection.Exceptions, collection.Exceptions);
        }
    }

    [Fact]
    public void ExceptionCollection_Ctor_WithExceptions()
    {
        var exceptions = new List<Exception> { new InvalidOperationException() };
        ExceptionCollection collection = new(exceptions);
        Assert.NotNull(collection.Exceptions);
        Assert.Single(collection.Exceptions);
        Assert.IsType<InvalidOperationException>(collection.Exceptions[0]);
    }

    [Theory]
    [BoolData]
    public void ExceptionCollection_Serialize_ThrowsSerializationException(bool formatterEnabled)
    {
        using BinaryFormatterScope formatterScope = new(enable: formatterEnabled);
        using MemoryStream stream = new();
        BinaryFormatter formatter = new();
        ExceptionCollection collection = new(new List<Exception>());
        if (formatterEnabled)
        {
            Assert.Throws<SerializationException>(() => formatter.Serialize(stream, collection));
        }
        else
        {
            Assert.Throws<NotSupportedException>(() => formatter.Serialize(stream, collection));
        }
    }

    [Fact]
    public void ExceptionCollection_GetObjectData_ThrowsPlatformNotSupportedException()
    {
        ExceptionCollection collection = new(new List<Exception>());
        Assert.Throws<PlatformNotSupportedException>(() => collection.GetObjectData(null, default));
    }
}
