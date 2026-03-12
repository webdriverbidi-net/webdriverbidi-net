// <copyright file="CoreConceptsCustomModuleSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Custom module types for core-concepts advanced section snippets.

#pragma warning disable CS1591, CS0168, CS8600, CS8618
#pragma warning disable SYSLIB1038

namespace WebDriverBiDi.Docs.Code.CoreConcepts;

using System.Text.Json.Serialization;
using WebDriverBiDi;
using WebDriverBiDi.Protocol;

/// <summary>
/// Example custom module for documentation.
/// </summary>
#region CreatingaCustomModule
public class CustomModule : Module
{
    // "custom" is the protocol module name
    public const string CustomModuleName = "custom";

    public CustomModule(IBiDiCommandExecutor driver)
        : base(driver)
    {
    }

    public override string ModuleName => CustomModuleName;

    // Define custom commands
    public async Task<CustomCommandResult> MyCustomCommandAsync(
        CustomCommandParameters parameters)
    {
        return await this.Driver.ExecuteCommandAsync<CustomCommandResult>(
            parameters);
    }

    // Define custom events
    public ObservableEvent<CustomEventArgs> OnCustomEvent { get; } =
        new ObservableEvent<CustomEventArgs>("custom.eventOccurred");
}

// Command parameters (mutable - sent to browser)
public class CustomCommandParameters : CommandParameters<CustomCommandResult>
{
    public CustomCommandParameters()
        : base()  // Protocol method name
    {
    }

    public string CustomProperty { get; set; }

    public override string MethodName => "custom.myCommand";
}

// Command result (immutable - received from browser)
public record CustomCommandResult : CommandResult
{
    public string ResultData { get; internal set; }
}

// Event arguments (immutable - received from browser)
public record CustomEventArgs : WebDriverBiDiEventArgs
{
    public string EventData { get; internal set; }
}
#endregion

#region CustomJSONTypeResolvers
[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(CustomCommandParameters))]
[JsonSerializable(typeof(CommandResponseMessage<CustomCommandResult>))]
[JsonSerializable(typeof(CustomCommandResult))]
[JsonSerializable(typeof(EventMessage<CustomEventArgs>))]
[JsonSerializable(typeof(CustomEventArgs))]
internal partial class CustomJsonContext : JsonSerializerContext
{
}
#endregion

#region CustomTypesforAOT
public class MyCommandParameters : CommandParameters<MyCommandResult>
{
    [JsonIgnore]
    public override string MethodName => "myModule.myCommand";

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public record MyCommandResult : CommandResult
{
    [JsonIgnore]
    public override bool IsError => false;

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;
}

public record MyEventArgs : WebDriverBiDiEventArgs
{
    [JsonPropertyName("detail")]
    public string Detail { get; set; } = string.Empty;
}
#endregion

/// <summary>
/// Example JSON source generation context for AOT documentation.
/// </summary>
// Define JSON source generation context for custom types

#region Source-GeneratedContext
[JsonSerializable(typeof(MyCommandParameters))]
[JsonSerializable(typeof(CommandResponseMessage<MyCommandResult>))]
[JsonSerializable(typeof(EventMessage<MyEventArgs>))]
public partial class MyModuleJsonSerializerContext : JsonSerializerContext
{
}
#endregion

/// <summary>
/// Referenced in AotCompatibilitySamples.cs - shows registering custom module and JSON context for AOT.
/// </summary>
public class MyCustomModule : Module
{
    public const string ModuleNameConst = "myModule";

    public MyCustomModule(IBiDiCommandExecutor driver)
        : base(driver)
    {
    }

    public override string ModuleName => ModuleNameConst;

    public async Task<MyCommandResult> MyCommandAsync(MyCommandParameters parameters)
    {
        return await this.Driver.ExecuteCommandAsync<MyCommandResult>(parameters);
    }

    public ObservableEvent<MyEventArgs> OnMyEvent { get; } =
        new ObservableEvent<MyEventArgs>("myModule.myEvent");
}

#pragma warning restore CS1591, CS0168, CS8600, CS8618
#pragma warning restore SYSLIB1038 // Inaccessible properties annotated with the JsonIncludeAttribute are not supported in source generation mode.
