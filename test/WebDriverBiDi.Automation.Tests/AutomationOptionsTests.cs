// <copyright file="AutomationOptionsTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

public class AutomationOptionsTests
{
    public static TheoryData<TimeSpan> NonPositiveIntervals => [TimeSpan.Zero, TimeSpan.FromMilliseconds(-1)];

    [Fact]
    public void DefaultsAreSet()
    {
        AutomationOptions options = new();

        Assert.Equal(TimeSpan.FromSeconds(30), options.ActionTimeout);
        Assert.Equal(TimeSpan.FromSeconds(30), options.NavigationTimeout);
        Assert.Equal(TimeSpan.FromSeconds(5), options.ExpectTimeout);
        Assert.Equal(TimeSpan.FromMilliseconds(100), options.PollInterval);
        Assert.Same(TimeProvider.System, options.TimeProvider);
        Assert.Equal("webdriverbidi-automation", options.SandboxName);
    }

    [Fact]
    public void ValuesAreKept()
    {
        TimeProvider timeProvider = new ManualTimeProvider();

        AutomationOptions options = new()
        {
            ActionTimeout = TimeSpan.FromSeconds(1),
            NavigationTimeout = TimeSpan.FromSeconds(2),
            ExpectTimeout = TimeSpan.FromSeconds(3),
            PollInterval = TimeSpan.FromMilliseconds(4),
            TimeProvider = timeProvider,
            SandboxName = "custom",
        };

        Assert.Equal(TimeSpan.FromSeconds(1), options.ActionTimeout);
        Assert.Equal(TimeSpan.FromSeconds(2), options.NavigationTimeout);
        Assert.Equal(TimeSpan.FromSeconds(3), options.ExpectTimeout);
        Assert.Equal(TimeSpan.FromMilliseconds(4), options.PollInterval);
        Assert.Same(timeProvider, options.TimeProvider);
        Assert.Equal("custom", options.SandboxName);
    }

    [Theory]
    [MemberData(nameof(NonPositiveIntervals))]
    public void IntervalThatIsNotPositiveIsRejected(TimeSpan value)
    {
        Assert.Equal("ActionTimeout", Assert.Throws<ArgumentOutOfRangeException>(() => new AutomationOptions() { ActionTimeout = value }).ParamName);
        Assert.Equal("NavigationTimeout", Assert.Throws<ArgumentOutOfRangeException>(() => new AutomationOptions() { NavigationTimeout = value }).ParamName);
        Assert.Equal("ExpectTimeout", Assert.Throws<ArgumentOutOfRangeException>(() => new AutomationOptions() { ExpectTimeout = value }).ParamName);
        Assert.Equal("PollInterval", Assert.Throws<ArgumentOutOfRangeException>(() => new AutomationOptions() { PollInterval = value }).ParamName);
    }

    [Fact]
    public void MissingTimeProviderIsRejected()
    {
        Assert.Throws<ArgumentNullException>(() => new AutomationOptions() { TimeProvider = null! });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void EmptySandboxNameIsRejected(string? name)
    {
        Assert.Throws<ArgumentException>(() => new AutomationOptions() { SandboxName = name! });
    }

    private sealed class ManualTimeProvider : TimeProvider
    {
    }
}
