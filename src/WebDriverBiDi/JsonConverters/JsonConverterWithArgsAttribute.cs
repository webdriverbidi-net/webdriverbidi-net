// <copyright file="JsonConverterWithArgsAttribute.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// Recipe taken from here https://github.com/dotnet/runtime/issues/54187#issuecomment-871293887
namespace WebDriverBiDi.JsonConverters;

using System.Text.Json.Serialization;

/// <summary>
/// This attribute plays like the JsonConverter. but it takes constructor arguments to be passed to the converter.
/// </summary>
internal class JsonConverterWithArgsAttribute : JsonConverterAttribute
{
    private Type converterType;

    private object?[] converterArguments;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonConverterWithArgsAttribute"/> class.
    /// </summary>
    /// <param name="converterType">Converter type.</param>
    /// <param name="converterArguments">Constructor arguments.</param>
    public JsonConverterWithArgsAttribute(Type converterType, params object?[] converterArguments)
    {
        this.converterType = converterType;
        this.converterArguments = converterArguments;
    }

    /// <summary>
    /// Gets the converter type.
    /// </summary>
    public new Type ConverterType => this.converterType;

    /// <summary>
    /// Gets the converter's constructor arguments.
    /// </summary>
    public object?[] ConverterArguments => this.converterArguments;

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(Type type)
    {
        return (JsonConverter)Activator.CreateInstance(this.converterType, this.converterArguments)!;
    }
}