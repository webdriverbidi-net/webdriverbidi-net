// * MIT License
//  *
//  * Copyright (c) Darío Kondratiuk
//  *
//  * Permission is hereby granted, free of charge, to any person obtaining a copy
//  * of this software and associated documentation files (the "Software"), to deal
//  * in the Software without restriction, including without limitation the rights
//  * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  * copies of the Software, and to permit persons to whom the Software is
//  * furnished to do so, subject to the following conditions:
//  *
//  * The above copyright notice and this permission notice shall be included in all
//  * copies or substantial portions of the Software.
//  *
//  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  * SOFTWARE.

using System.Text.Json.Serialization;

namespace WebDriverBiDi.JsonConverters;

// Recipe taken from here https://github.com/dotnet/runtime/issues/54187#issuecomment-871293887
/// <summary>
/// This attribute plays like the JsonConverter. but it takes constructor arguments to be passed to the converter.
/// </summary>
internal class JsonConverterWithArgsAttribute : JsonConverterAttribute
{
    private Type converterType;

    private object?[] converterArguments;

    public JsonConverterWithArgsAttribute(Type converterType, params object?[] converterArguments)
    {
        this.converterType = converterType;
        this.converterArguments = converterArguments;
    }

    // CreateConverter method is only called if base.ConverterType is null 
    // so store the type parameter in a new property in the derived attribute
    // https://github.com/dotnet/runtime/blob/57bfe474518ab5b7cfe6bf7424a79ce3af9d6657/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/JsonSerializerOptions.Converters.cs#L278
    public new Type ConverterType => this.converterType;
    
    public object?[] ConverterArguments => this.converterArguments;

    public override JsonConverter? CreateConverter(Type _)
    {
        return (JsonConverter)Activator.CreateInstance(ConverterType, ConverterArguments)!;
    }
}