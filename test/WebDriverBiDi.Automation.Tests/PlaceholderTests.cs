// <copyright file="PlaceholderTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

// The test platform fails a run that finds no tests. Remove this once Phase 2 adds real tests.
public class PlaceholderTests
{
    [Fact]
    public void AutomationAssemblyLoads()
    {
        Assert.Equal("WebDriverBiDi.Automation", typeof(Browser).Assembly.GetName().Name);
    }
}
