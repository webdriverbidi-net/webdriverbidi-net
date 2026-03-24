// <copyright file="AotCompatibilitySamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/advanced/aot-compatibility.md

#pragma warning disable CS8600, CS8602, CS8618, SYSLIB1038

namespace WebDriverBiDi.Docs.Code.Advanced;

using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Docs.Code.CoreConcepts;
using WebDriverBiDi.Protocol;

/// <summary>
/// Snippets for AOT compatibility documentation.
/// </summary>
public static class AotCompatibilitySamples
{
    /// <summary>
    /// Built-in modules work in AOT with no extra setup.
    /// </summary>
    public static async Task BuiltInModulesInAot(string webSocketUrl, string contextId)
    {
        #region Built-inModulesinAOT
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync(webSocketUrl);

        // All built-in modules work in AOT with no extra setup
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com"));
        #endregion
    }

    /// <summary>
    /// Register custom resolver and connect.
    /// </summary>
    public static async Task RegisterAndConnect(string webSocketUrl)
    {
        #region RegisterandConnect
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

        // Register your serialization metadata for AOT support
        await driver.RegisterTypeInfoResolver(MyModuleJsonSerializerContext.Default);

        // Register your custom module
        MyCustomModule myModule = new MyCustomModule(driver);
        driver.RegisterModule(myModule);

        // Now connect — the transport will use both the built-in and your custom metadata
        await driver.StartAsync(webSocketUrl);
        #endregion
    }

    public static async Task MultipleContextsOptionARegistration(BiDiDriver driver)
    {
        #region MultipleCustomModulesOptionARegistration
        // Register once
        await driver.RegisterTypeInfoResolver(AllCustomModulesJsonContext.Default);
        #endregion
    }

    public static async Task MultipleContextsOptionBRegistration(BiDiDriver driver)
    {
        #region MultipleCustomModulesOptionBRegistration
        // Option B: Separate contexts per module
        await driver.RegisterTypeInfoResolver(ModuleAJsonContext.Default);
        await driver.RegisterTypeInfoResolver(ModuleBJsonContext.Default);
        #endregion
    }

    /// <summary>
    /// Consumer registers extension context.
    /// </summary>
    public static async Task ConsumerRegistersExtension(
        BiDiDriver driver)
    {
        #region ConsumerRegistersExtension
        await driver.RegisterTypeInfoResolver(MyExtensionJsonSerializerContext.Default);
        driver.RegisterModule(new MyExtensionModule(driver));
        #endregion
    }
}

[JsonSerializable(typeof(MyExtensionCommandParameters))]
internal partial class MyExtensionJsonSerializerContext : JsonSerializerContext { }

public class MyExtensionModule : Module
{
    public MyExtensionModule(IBiDiCommandExecutor driver) : base(driver)
    {
    }

    public override string ModuleName => throw new NotImplementedException();
}

public class MyExtensionCommandParameters : CommandParameters<CommandResult>
{
    [JsonIgnore]
    public override string MethodName => throw new NotImplementedException();

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public enum MyCustomEnum
{
    ValueA,
    ValueB,
}

/// <summary>
/// Sample enum for AOT enum rooting snippet.
/// </summary>
public enum AotSampleEnum
{
    ValueA,
    ValueB,
}

/// <summary>
/// Sample command parameters using enum - for AOT enum rooting snippet.
/// </summary>
public class AotSampleCommandParameters
{
    public AotSampleEnum MyEnum { get; set; }
}

/// <summary>
/// AOT enum fix: root enum array type via static constructor.
/// Use this pattern when custom types use enums with custom JsonConverter.
/// </summary>
#region AOTEnumRooting
[JsonSerializable(typeof(AotSampleCommandParameters))]
public partial class AotSampleModuleJsonSerializerContext : JsonSerializerContext
{
    static AotSampleModuleJsonSerializerContext()
    {
        RuntimeHelpers.RunClassConstructor(typeof(AotSampleEnum[]).TypeHandle);
    }
}
#endregion

public class ModuleACommandParameters : CommandParameters<ModuleACommandResult>
{
    [JsonIgnore]
    public override string MethodName => "moduleA.command";

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public record ModuleACommandResult : CommandResult
{
    [JsonIgnore]
    public override bool IsError => false;

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;
}

public class ModuleBCommandParameters : CommandParameters<ModuleBCommandResult>
{
    [JsonIgnore]
    public override string MethodName => "moduleB.command";

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public record ModuleBCommandResult : CommandResult
{
    [JsonIgnore]
    public override bool IsError => false;

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;
}

public record ModuleBEventArgs : WebDriverBiDiEventArgs
{
    [JsonPropertyName("detail")]
    public string Detail { get; set; } = string.Empty;
}

#region MultipleCustomModulesOptionA
// Option A: One context for everything
[JsonSerializable(typeof(ModuleACommandParameters))]
[JsonSerializable(typeof(CommandResponseMessage<ModuleACommandResult>))]
[JsonSerializable(typeof(ModuleBCommandParameters))]
[JsonSerializable(typeof(CommandResponseMessage<ModuleBCommandResult>))]
[JsonSerializable(typeof(EventMessage<ModuleBEventArgs>))]
public partial class AllCustomModulesJsonContext : JsonSerializerContext { }
#endregion

[JsonSerializable(typeof(ModuleACommandParameters))]
public partial class ModuleAJsonContext : JsonSerializerContext { }

[JsonSerializable(typeof(ModuleBCommandParameters))]
public partial class ModuleBJsonContext : JsonSerializerContext { }
