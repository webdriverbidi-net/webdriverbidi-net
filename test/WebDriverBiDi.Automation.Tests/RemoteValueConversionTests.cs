// <copyright file="RemoteValueConversionTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Numerics;
using System.Text.Json.Nodes;
using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.Script;

public class RemoteValueConversionTests
{
    [Fact]
    public async Task ScalarsConvertToTheirTypes()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        Assert.Equal("text", await EvaluateAsync<string>(session, page, String("text")));
        Assert.True(await EvaluateAsync<bool>(session, page, new JsonObject() { ["type"] = "boolean", ["value"] = true }));
        Assert.Equal(new DateTime(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc), (await EvaluateAsync<DateTime>(session, page, new JsonObject() { ["type"] = "date", ["value"] = "2026-09-30T12:00:00.000Z" })).ToUniversalTime());
        Assert.Equal(2.5, await EvaluateAsync<double>(session, page, Number(2.5)));
        Assert.Equal(2.5f, await EvaluateAsync<float>(session, page, Number(2.5)));
        Assert.Equal(2.5m, await EvaluateAsync<decimal>(session, page, Number(2.5)));
        Assert.Equal(new BigInteger(12345678901234567890), await EvaluateAsync<BigInteger>(session, page, BigInt("12345678901234567890")));
        Assert.Equal(new BigInteger(7), await EvaluateAsync<BigInteger>(session, page, Number(7)));
    }

    [Fact]
    public async Task WholeNumbersConvertToEveryIntegerType()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        Assert.Equal(-7L, await EvaluateAsync<long>(session, page, Number(-7)));
        Assert.Equal(-7, await EvaluateAsync<int>(session, page, Number(-7)));
        Assert.Equal((short)-7, await EvaluateAsync<short>(session, page, Number(-7)));
        Assert.Equal((sbyte)-7, await EvaluateAsync<sbyte>(session, page, Number(-7)));
        Assert.Equal(7UL, await EvaluateAsync<ulong>(session, page, Number(7)));
        Assert.Equal(7U, await EvaluateAsync<uint>(session, page, Number(7)));
        Assert.Equal((ushort)7, await EvaluateAsync<ushort>(session, page, Number(7)));
        Assert.Equal((byte)7, await EvaluateAsync<byte>(session, page, Number(7)));
        Assert.Equal(9007199254740993L, await EvaluateAsync<long>(session, page, BigInt("9007199254740993")));
    }

    [Fact]
    public async Task NumbersThatDoNotFitAreRejected()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        InvalidCastException fraction = await Assert.ThrowsAsync<InvalidCastException>(() => EvaluateAsync<int>(session, page, Number(1.5)));
        await Assert.ThrowsAsync<OverflowException>(() => EvaluateAsync<byte>(session, page, Number(300)));
        await Assert.ThrowsAsync<OverflowException>(() => EvaluateAsync<int>(session, page, SpecialNumber("NaN")));
        await Assert.ThrowsAsync<OverflowException>(() => EvaluateAsync<long>(session, page, SpecialNumber("Infinity")));
        await Assert.ThrowsAsync<OverflowException>(() => EvaluateAsync<decimal>(session, page, SpecialNumber("NaN")));
        await Assert.ThrowsAsync<OverflowException>(() => EvaluateAsync<decimal>(session, page, SpecialNumber("-Infinity")));
        await Assert.ThrowsAsync<OverflowException>(() => EvaluateAsync<int>(session, page, BigInt("9007199254740993")));

        Assert.Equal("The script returned 1.5, which is not a whole number, so it cannot be converted to System.Int32.", fraction.Message);
        Assert.True(double.IsNaN(await EvaluateAsync<double>(session, page, SpecialNumber("NaN"))));
    }

    [Fact]
    public async Task ValuesOfTheWrongKindAreRejected()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        InvalidCastException text = await Assert.ThrowsAsync<InvalidCastException>(() => EvaluateAsync<int>(session, page, String("5")));
        await Assert.ThrowsAsync<InvalidCastException>(() => EvaluateAsync<string>(session, page, Number(5)));
        await Assert.ThrowsAsync<InvalidCastException>(() => EvaluateAsync<double>(session, page, BigInt("5")));
        await Assert.ThrowsAsync<InvalidCastException>(() => EvaluateAsync<int>(session, page, Null()));
        await Assert.ThrowsAsync<InvalidCastException>(() => EvaluateAsync<NumberRemoteValue>(session, page, String("5")));
        await Assert.ThrowsAsync<InvalidCastException>(() => EvaluateAsync<int[]>(session, page, Number(5)));
        await Assert.ThrowsAsync<InvalidCastException>(() => EvaluateAsync<Dictionary<string, int>>(session, page, Array(Number(1))));
        await Assert.ThrowsAsync<InvalidCastException>(() => EvaluateAsync<object>(session, page, Node()));

        Assert.Equal("The script returned a value of type String, which cannot be converted to System.Int32.", text.Message);
    }

    [Fact]
    public async Task TypesThatAreNotSupportedAreRejectedWhateverTheValue()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        NotSupportedException guid = await Assert.ThrowsAsync<NotSupportedException>(() => EvaluateAsync<Guid>(session, page, String("x")));
        await Assert.ThrowsAsync<NotSupportedException>(() => EvaluateAsync<Guid>(session, page, Number(1)));
        await Assert.ThrowsAsync<NotSupportedException>(() => EvaluateAsync<TimeSpan>(session, page, Number(1.5)));
        await Assert.ThrowsAsync<NotSupportedException>(() => EvaluateAsync<Guid>(session, page, BigInt("1")));
        NotSupportedException nested = await Assert.ThrowsAsync<NotSupportedException>(() => EvaluateAsync<List<int>[]>(session, page, Array(Array(Number(1)))));
        await Assert.ThrowsAsync<NotSupportedException>(() => EvaluateAsync<Dictionary<int, string>>(session, page, Object()));

        Assert.Equal("System.Guid is not a type a script result converts to.", guid.Message);
        Assert.Equal("A System.Collections.Generic.List`1[System.Int32] within another value cannot be created; use an array, or object for an untyped value.", nested.Message);
    }

    [Fact]
    public async Task NullAndUndefinedBecomeNullForTypesThatAllowIt()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        Assert.Null(await EvaluateAsync<int?>(session, page, Null()));
        Assert.Equal(5, await EvaluateAsync<int?>(session, page, Number(5)));
        Assert.Null(await EvaluateAsync<string?>(session, page, Undefined()));
        Assert.Null(await EvaluateAsync<object?>(session, page, Null()));
        Assert.Null(await EvaluateAsync<List<int>?>(session, page, Undefined()));
        Assert.Null(await EvaluateAsync<Dictionary<string, int>?>(session, page, Null()));
        Assert.IsType<NullRemoteValue>(await EvaluateAsync<RemoteValue>(session, page, Null()));
    }

    [Fact]
    public async Task ArraysOfAnyDepthAreCreated()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        int[] numbers = await EvaluateAsync<int[]>(session, page, Array(Number(1), Number(2)));
        string[][] nested = await EvaluateAsync<string[][]>(session, page, Array(Array(String("a")), Array()));
        int?[] withNull = await EvaluateAsync<int?[]>(session, page, Array(Number(1), Null()));

        Assert.Equal([1, 2], numbers);
        Assert.Equal(["a"], nested[0]);
        Assert.Empty(nested[1]);
        Assert.Equal([1, null], withNull);
    }

    [Fact]
    public async Task ListsAndDictionariesAreCreatedAtTheTopLevel()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        List<string> letters = await EvaluateAsync<List<string>>(session, page, new JsonObject() { ["type"] = "set", ["value"] = new JsonArray(String("a"), String("b")) });
        Dictionary<string, double[]> fromObject = await EvaluateAsync<Dictionary<string, double[]>>(session, page, Object(("x", Array(Number(1.5)))));
        Dictionary<string, int> fromMap = await EvaluateAsync<Dictionary<string, int>>(session, page, Map((String("k"), Number(3))));
        InvalidCastException keyNotText = await Assert.ThrowsAsync<InvalidCastException>(() => EvaluateAsync<Dictionary<string, int>>(session, page, Map((Number(1), Number(3)))));

        Assert.Equal(["a", "b"], letters);
        Assert.Equal([1.5], fromObject["x"]);
        Assert.Equal(3, fromMap["k"]);
        Assert.Equal("The script returned a map with a key that is not a string, so it cannot be converted to System.Collections.Generic.Dictionary`2[System.String,System.Int32].", keyNotText.Message);
    }

    [Fact]
    public async Task ObjectBecomesAnUntypedTree()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        object? tree = await EvaluateAsync<object?>(session, page, Object(
            ("text", String("t")),
            ("number", Number(1)),
            ("flag", new JsonObject() { ["type"] = "boolean", ["value"] = false }),
            ("big", BigInt("2")),
            ("when", new JsonObject() { ["type"] = "date", ["value"] = "2026-09-30T12:00:00.000Z" }),
            ("list", Array(Null(), Undefined()))));

        Dictionary<string, object?> dictionary = Assert.IsType<Dictionary<string, object?>>(tree);
        Assert.Equal("t", dictionary["text"]);
        Assert.Equal(1.0, dictionary["number"]);
        Assert.Equal(false, dictionary["flag"]);
        Assert.Equal(new BigInteger(2), dictionary["big"]);
        Assert.IsType<DateTime>(dictionary["when"]);
        Assert.Equal([null, null], Assert.IsType<List<object?>>(dictionary["list"]));
    }

    [Fact]
    public async Task RemoteValueTypesAreReturnedUnconverted()
    {
        (BiDiDriver driver, FakeSession session, Page page) = await OpenPageAsync();
        await using BiDiDriver ownedDriver = driver;

        Assert.IsType<NodeRemoteValue>(await EvaluateAsync<NodeRemoteValue>(session, page, Node()));
        Assert.IsType<NodeRemoteValue>(await EvaluateAsync<RemoteValue>(session, page, Node()));
        RemoteValue[] nodes = await EvaluateAsync<RemoteValue[]>(session, page, Array(Node(), Node()));
        Assert.Equal(2, nodes.Length);
    }

    private static async Task<(BiDiDriver Driver, FakeSession Session, Page Page)> OpenPageAsync()
    {
        (BiDiDriver driver, FakeSession session) = await FakeSession.ConnectAsync();
        BrowserGroup group = await BrowserGroup.ConnectAsync(driver, cancellationToken: TestContext.Current.CancellationToken);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        return (driver, session, page);
    }

    private static Task<T> EvaluateAsync<T>(FakeSession session, Page page, JsonObject result)
    {
        session.RemoteEnd.AnswerWith("script.callFunction", ProtocolJson.Success(result));
        return page.EvaluateAsync<T>("() => value", cancellationToken: TestContext.Current.CancellationToken);
    }

    private static JsonObject String(string value) => new() { ["type"] = "string", ["value"] = value };

    private static JsonObject Number(double value) => new() { ["type"] = "number", ["value"] = value };

    private static JsonObject SpecialNumber(string value) => new() { ["type"] = "number", ["value"] = value };

    private static JsonObject BigInt(string value) => new() { ["type"] = "bigint", ["value"] = value };

    private static JsonObject Null() => new() { ["type"] = "null" };

    private static JsonObject Undefined() => new() { ["type"] = "undefined" };

    private static JsonObject Node() => new() { ["type"] = "node", ["sharedId"] = "node-1", ["value"] = new JsonObject() { ["nodeType"] = 1, ["childNodeCount"] = 0 } };

    private static JsonObject Array(params JsonObject[] items) => new() { ["type"] = "array", ["value"] = new JsonArray([.. items]) };

    private static JsonObject Object(params (string Key, JsonObject Value)[] entries) => new() { ["type"] = "object", ["value"] = new JsonArray([.. entries.Select(entry => (JsonNode)new JsonArray(entry.Key, entry.Value))]) };

    private static JsonObject Map(params (JsonObject Key, JsonObject Value)[] entries) => new() { ["type"] = "map", ["value"] = new JsonArray([.. entries.Select(entry => (JsonNode)new JsonArray(entry.Key, entry.Value))]) };
}
