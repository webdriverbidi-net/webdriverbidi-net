// <copyright file="RegistrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using global::Xunit;

public class RegistrationTests
{
    [Fact]
    public async Task XunitSharedGroupNeedsTheAssemblyFixture()
    {
        XunitTest test = new();

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await test.InitializeAsync());

        Assert.Equal(
            $"Dramaturge's shared browser group is not registered. Add this line to the test project, or name a class derived from DramaturgeAssemblyFixture in it:{Environment.NewLine}[assembly: AssemblyFixture(typeof(Dramaturge.Xunit.DramaturgeAssemblyFixture))]",
            exception.Message);
    }

    [Fact]
    public async Task NUnitSharedGroupNeedsTheSetUpFixture()
    {
        NUnitTest test = new();

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(test.SetUpBrowsersAsync);

        Assert.Equal(
            $"Dramaturge's shared browser group is not registered. Add this class to the test project, outside any namespace, or derive one from DramaturgeSetUpFixture in it:{Environment.NewLine}[SetUpFixture] public class DramaturgeSetUp : Dramaturge.NUnit.DramaturgeSetUpFixture {{ }}",
            exception.Message);
    }

    [Fact]
    public async Task MSTestSharedGroupNeedsTheAssemblyFixture()
    {
        MSTestTest test = new();

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(test.SetUpBrowsersAsync);

        string expectedClass = string.Join(
            Environment.NewLine,
            "[TestClass]",
            "public static class DramaturgeSetUp",
            "{",
            "    [AssemblyInitialize]",
            "    public static void Initialize(TestContext context) => Dramaturge.MSTest.DramaturgeAssemblyFixture.Register(new());",
            string.Empty,
            "    [AssemblyCleanup]",
            "    public static Task CleanupAsync() => Dramaturge.MSTest.DramaturgeAssemblyFixture.CloseAsync();",
            "}");
        Assert.Equal(
            $"Dramaturge's shared browser group is not registered. Add this class to the test project, or register a class derived from DramaturgeAssemblyFixture in it:{Environment.NewLine}{expectedClass}",
            exception.Message);
    }

    [Fact]
    public async Task TUnitSharedGroupNeedsTheAssemblyFixture()
    {
        TUnitTest test = new();

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(test.SetUpBrowsersAsync);

        string expectedClass = string.Join(
            Environment.NewLine,
            "public static class DramaturgeSetUp",
            "{",
            "    [Before(HookType.TestSession)]",
            "    public static void Register() => Dramaturge.TUnit.DramaturgeAssemblyFixture.Register(new());",
            string.Empty,
            "    [After(HookType.TestSession)]",
            "    public static Task CloseAsync() => Dramaturge.TUnit.DramaturgeAssemblyFixture.CloseAsync();",
            "}");
        Assert.Equal(
            $"Dramaturge's shared browser group is not registered. Add this class to the test project, or register a class derived from DramaturgeAssemblyFixture in it:{Environment.NewLine}{expectedClass}",
            exception.Message);
    }

    [Fact]
    public async Task MSTestClosingAnUnusedSharedGroupRemovesItsRegistration()
    {
        MSTest.DramaturgeAssemblyFixture.Register(new MSTest.DramaturgeAssemblyFixture());

        await MSTest.DramaturgeAssemblyFixture.CloseAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(new MSTestTest().SetUpBrowsersAsync);
    }

    [Fact]
    public async Task TUnitClosingAnUnusedSharedGroupRemovesItsRegistration()
    {
        TUnit.DramaturgeAssemblyFixture.Register(new TUnit.DramaturgeAssemblyFixture());

        await TUnit.DramaturgeAssemblyFixture.CloseAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(new TUnitTest().SetUpBrowsersAsync);
    }

    private sealed class TUnitTest : TUnit.BrowserTest
    {
    }

    private sealed class MSTestTest : MSTest.BrowserTest
    {
    }

    private sealed class XunitTest : Xunit.BrowserTest
    {
    }

    private sealed class NUnitTest : NUnit.BrowserTest
    {
    }
}
