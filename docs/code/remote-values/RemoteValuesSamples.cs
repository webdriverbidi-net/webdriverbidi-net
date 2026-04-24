// <copyright file="RemoteValuesSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/remote-values.md

#pragma warning disable CS1591, CS0168, CS0219, CS8600, CS8602, CS8604 // Possible null reference argument.

namespace WebDriverBiDi.Docs.Code.RemoteValues;

using System.Collections.Generic;
using System.Linq;
using WebDriverBiDi;
using WebDriverBiDi.Script;

/// <summary>
/// Snippets for remote values documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class RemoteValuesSamples
{
    /// <summary>
    /// ValueAs with number.
    /// </summary>
    public static async Task ValueAsNumber(
        BiDiDriver driver,
        Target target)
    {
        #region ValueAsNumber
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters("42", target, true));

        if (result is EvaluateResultSuccess success)
        {
            RemoteValue remoteValue = success.Result;

            // Convert to long
            long number = remoteValue.ConvertTo<NumberRemoteValue>();
            Console.WriteLine(number); // 42

            // Can also convert to double
            double doubleNumber = remoteValue.ConvertTo<NumberRemoteValue>();
            Console.WriteLine(doubleNumber); // 42.0
        }
        #endregion
    }

    /// <summary>
    /// ValueAs with string.
    /// </summary>
    public static async Task ValueAsString(
        BiDiDriver driver,
        Target target)
    {
        #region ValueAsString
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters("'Hello, World!'", target, true));

        if (result is EvaluateResultSuccess success &&
            success.Result is StringRemoteValue stringValue)
        {
            string text = stringValue.Value;
            Console.WriteLine(text); // "Hello, World!"
        }
        #endregion
    }

    /// <summary>
    /// ValueAs with boolean.
    /// </summary>
    public static async Task ValueAsBoolean(
        BiDiDriver driver,
        Target target)
    {
        #region ValueAsBoolean
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters("true", target, true));

        if (result is EvaluateResultSuccess success &&
            success.Result is BooleanRemoteValue booleanValue)
        {
            bool flag = booleanValue.Value;
            Console.WriteLine(flag); // True
        }
        #endregion
    }

    /// <summary>
    /// Null and undefined check.
    /// </summary>
    public static async Task NullAndUndefined(
        BiDiDriver driver,
        Target target)
    {
        #region NullandUndefined
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters("null", target, true));

        if (result is EvaluateResultSuccess success)
        {
            if (success.Result.Type == RemoteValueType.Null || success.Result.Type == RemoteValueType.Undefined)
            {
                Console.WriteLine("Value is null or undefined");
            }
        }
        #endregion
    }

    /// <summary>
    /// Simple object with RemoteValueDictionary.
    /// </summary>
    public static async Task SimpleObject(
        BiDiDriver driver,
        Target target)
    {
        #region SimpleObject
        string script = """
        (
            name: 'John',
            age: 30,
            active: true
        }
        """;

        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(script, target, true));

        if (result is EvaluateResultSuccess success)
        {
            KeyValuePairCollectionRemoteValue obj = success.Result.ConvertTo<KeyValuePairCollectionRemoteValue>();

            // Convert to RemoteValueDictionary; extract values with ValueAs<T>()
            RemoteValueDictionary dict = obj.Value;

            Console.WriteLine(dict["name"].ConvertTo<StringRemoteValue>().Value);   // "John"
            Console.WriteLine(dict["age"].ConvertTo<NumberRemoteValue>().Value);    // 30
            Console.WriteLine(dict["active"].ConvertTo<BooleanRemoteValue>().Value); // True
        }
        #endregion
    }

    /// <summary>
    /// Nested objects with RemoteValueDictionary.
    /// </summary>
    public static async Task NestedObjects(
        BiDiDriver driver,
        Target target)
    {
        #region NestedObjects
        string script = """
            (
                user: {
                    name: 'John',
                    address: {
                        city: 'New York',
                        zip: '10001'
                    }
                }
            }
            """;

        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(script, target, true));

        if (result is EvaluateResultSuccess success &&
            success.Result is KeyValuePairCollectionRemoteValue dictionaryValue)
        {
            RemoteValueDictionary dict = dictionaryValue.Value;
            RemoteValueDictionary user = dict["user"].ConvertTo<KeyValuePairCollectionRemoteValue>().Value;
            RemoteValueDictionary address = user["address"].ConvertTo<KeyValuePairCollectionRemoteValue>().Value;

            Console.WriteLine(address["city"].ConvertTo<StringRemoteValue>().Value); // "New York"
        }
        #endregion
    }

    /// <summary>
    /// Array with RemoteValueList.
    /// </summary>
    public static async Task ArrayWithRemoteValueList(
        BiDiDriver driver,
        Target target)
    {
        #region ArraywithRemoteValueList
        string script = "[1, 2, 3, 4, 5]";

        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(script, target, true));

        if (result is EvaluateResultSuccess success &&
            success.Result is CollectionRemoteValue listValue)
        {
            RemoteValueList list = listValue.Value;

            Console.WriteLine($"Length: {list.Count}"); // 5

            foreach (RemoteValue item in list)
            {
                Console.WriteLine(item.ConvertTo<NumberRemoteValue>().Value);
            }
        }
        #endregion
    }

    /// <summary>
    /// Array of objects.
    /// </summary>
    public static async Task ArrayOfObjects(
        BiDiDriver driver,
        Target target)
    {
        #region ArrayofObjects
        string script = """
            [
                { name: 'Alice', age: 25 },
                { name: 'Bob', age: 30 },
                { name: 'Charlie', age: 35 }
            ]
        """;

        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(script, target, true));

        if (result is EvaluateResultSuccess success &&
            success.Result is CollectionRemoteValue listValue)
        {
            RemoteValueList list = listValue.Value;

            foreach (RemoteValue item in list)
            {
                RemoteValueDictionary person = item.ConvertTo<KeyValuePairCollectionRemoteValue>().Value;
                Console.WriteLine($"{person["name"].ConvertTo<StringRemoteValue>().Value}, age {person["age"].ConvertTo<NumberRemoteValue>().Value}");
            }
        }
        #endregion
    }

    /// <summary>
    /// ToObject recursive helper for flattening RemoteValue.
    /// </summary>
    #region ToObjectHelper
    static object? ToObject(RemoteValue value)
    {
        return value.Type switch
        {
            RemoteValueType.String => value.ConvertTo<StringRemoteValue>().Value,
            RemoteValueType.Number => value.ConvertTo<NumberRemoteValue>().Value,
            RemoteValueType.Boolean => value.ConvertTo<BooleanRemoteValue>().Value,
            RemoteValueType.Null or RemoteValueType.Undefined => null,
            RemoteValueType.Object or RemoteValueType.Map => value.ConvertTo<KeyValuePairCollectionRemoteValue>().Value
                .ToDictionary(kvp => kvp.Key.ToString() ?? "", kvp => ToObject(kvp.Value)),
            RemoteValueType.Array or RemoteValueType.Set => value.ConvertTo<CollectionRemoteValue>().Value
                .Select(ToObject)
                .ToList(),
            _ => (value as ValueHoldingRemoteValue)?.ValueObject ?? "(object)"
        };
    }
    #endregion

    /// <summary>
    /// Usage: convert RemoteValueDictionary to Dictionary.
    /// </summary>
    public static void ToObjectUsage(EvaluateResultSuccess success)
    {
        #region ToObjectUsage
        // Usage: convert RemoteValueDictionary to Dictionary<string, object>
        RemoteValueDictionary dict = success.Result.ConvertTo<KeyValuePairCollectionRemoteValue>().Value;
        Dictionary<string, object?> flat = dict.ToDictionary(
            kvp => kvp.Key.ToString() ?? "",
            kvp => ToObject(kvp.Value));
        #endregion
    }

    /// <summary>
    /// Getting element information with NodeProperties.
    /// </summary>
    public static async Task GettingElementInformation(
        BiDiDriver driver,
        Target target)
    {
        #region GettingElementInformation
        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "document.querySelector('button')",
                target,
                true));

        if (result is EvaluateResultSuccess success)
        {
            RemoteValue elementRemoteValue = success.Result;
            elementRemoteValue.TryConvertTo(out NodeRemoteValue element);

            Console.WriteLine($"Type: {element.Type}"); // "node"
            Console.WriteLine($"SharedId: {element.SharedId}");

            // Get node properties
            NodeProperties nodeProps = element.Value;

            Console.WriteLine($"Tag: {nodeProps.LocalName}");
            Console.WriteLine($"Node Type: {nodeProps.NodeType}");
            Console.WriteLine($"Child Count: {nodeProps.ChildNodeCount}");

            // Get attributes
            if (nodeProps.Attributes != null)
            {
                foreach (var attr in nodeProps.Attributes)
                {
                    Console.WriteLine($"  {attr.Key} = {attr.Value}");
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// ToSharedReference for element.
    /// </summary>
    public static async Task ToSharedReference(
        BiDiDriver driver,
        Target target)
    {
        #region ToSharedReference
        // Get element
        EvaluateResult getResult = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "document.querySelector('button')",
                target,
                true));

        if (getResult is EvaluateResultSuccess getSuccess)
        {
            getSuccess.Result.TryConvertTo(out NodeRemoteValue? element);

            // Create a reference
            SharedReference elementRef = element.ToSharedReference();

            // Use in another script call
            CallFunctionCommandParameters clickParams = new CallFunctionCommandParameters(
                "(element) => element.click()",
                target,
                false);
            clickParams.Arguments.Add(elementRef);

            await driver.Script.CallFunctionAsync(clickParams);
        }
        #endregion
    }

    /// <summary>
    /// LocalValue primitives for CallFunction.
    /// </summary>
    public static async Task LocalValuePrimitives(
        BiDiDriver driver,
        Target target)
    {
        #region LocalValuePrimitives
        CallFunctionCommandParameters parameters = new CallFunctionCommandParameters(
            "(str, num, bool) => console.log(str, num, bool)",
            target,
            false);

        parameters.Arguments.Add(LocalValue.String("Hello"));
        parameters.Arguments.Add(LocalValue.Number(42));
        parameters.Arguments.Add(LocalValue.Boolean(true));

        await driver.Script.CallFunctionAsync(parameters);
        #endregion
    }

    /// <summary>
    /// LocalValue special values.
    /// </summary>
    public static void LocalValueSpecialValues(CallFunctionCommandParameters parameters)
    {
        #region LocalValueSpecialValues
        parameters.Arguments.Add(LocalValue.Null);
        parameters.Arguments.Add(LocalValue.Undefined);

        // Special numbers
        parameters.Arguments.Add(LocalValue.Number(double.PositiveInfinity));
        parameters.Arguments.Add(LocalValue.Number(double.NegativeInfinity));
        parameters.Arguments.Add(LocalValue.Number(double.NaN));
        #endregion
    }

    /// <summary>
    /// LocalValue object.
    /// </summary>
    public static void LocalValueObject(CallFunctionCommandParameters parameters)
    {
        #region LocalValueObject
        Dictionary<string, LocalValue> obj = new Dictionary<string, LocalValue>
        {
            { "name", LocalValue.String("John") },
            { "age", LocalValue.Number(30) },
            { "active", LocalValue.Boolean(true) },
            { "metadata", LocalValue.Object(new Dictionary<string, LocalValue>
                {
                    { "created", LocalValue.Date(DateTime.Now) }
                })
            }
        };

        parameters.Arguments.Add(LocalValue.Object(obj));
        #endregion
    }

    /// <summary>
    /// LocalValue array.
    /// </summary>
    public static void LocalValueArray(CallFunctionCommandParameters parameters)
    {
        #region LocalValueArray
        List<LocalValue> array = new List<LocalValue>
        {
            LocalValue.Number(1),
            LocalValue.Number(2),
            LocalValue.Number(3)
        };

        parameters.Arguments.Add(LocalValue.Array(array));
        #endregion
    }

    /// <summary>
    /// LocalValue Date.
    /// </summary>
    public static void LocalValueDate(CallFunctionCommandParameters parameters)
    {
        #region LocalValueDate
        parameters.Arguments.Add(LocalValue.Date(DateTime.Now));
        parameters.Arguments.Add(LocalValue.Date(new DateTime(2024, 1, 1)));
        #endregion
    }

    /// <summary>
    /// LocalValue RegularExpression.
    /// </summary>
    public static void LocalValueRegExp(CallFunctionCommandParameters parameters)
    {
        #region LocalValueRegExp
        parameters.Arguments.Add(LocalValue.RegExp("\\d+", "g"));
        parameters.Arguments.Add(LocalValue.RegExp("[a-z]+", "i"));
        #endregion
    }

    /// <summary>
    /// Safe type conversion with switch on value.Type.
    /// </summary>
    public static void SafeTypeConversion(EvaluateResultSuccess success)
    {
        #region SafeTypeConversion
        RemoteValue value = success.Result;

        switch (value.Type)
        {
            case RemoteValueType.String:
                string str = value.ConvertTo<StringRemoteValue>().Value;
                break;

            case RemoteValueType.Number:
                double num = value.ConvertTo<NumberRemoteValue>().Value;
                break;

            case RemoteValueType.Boolean:
                bool flag = value.ConvertTo<BooleanRemoteValue>().Value;
                break;

            case RemoteValueType.Object:
                RemoteValueDictionary obj = value.ConvertTo<KeyValuePairCollectionRemoteValue>().Value;
                break;

            case RemoteValueType.Array:
                RemoteValueList list = value.ConvertTo<CollectionRemoteValue>().Value;
                break;

            case RemoteValueType.Node:
                NodeProperties? node = value.ConvertTo<NodeRemoteValue>().Value;
                break;

            case RemoteValueType.Null:
            case RemoteValueType.Undefined:
                // Handle null/undefined
                break;
        }
        #endregion
    }

    /// <summary>
    /// Checking for specific types.
    /// </summary>
    public static void CheckingForSpecificTypes(RemoteValue value)
    {
        #region CheckingforSpecificTypes
        if (value.Type == RemoteValueType.Node)
        {
            // It's a DOM element
            value.TryConvertTo(out NodeRemoteValue? nodeValue);
            SharedReference elementRef = nodeValue.ToSharedReference();
        }
        else if (value.Type == RemoteValueType.Array)
        {
            // It's an array
            value.TryConvertTo(out CollectionRemoteValue? listValue);
            RemoteValueList list = listValue.Value;
        }
        #endregion
    }

    /// <summary>
    /// Extract multiple values from object.
    /// </summary>
    public static async Task ExtractMultipleValues(
        BiDiDriver driver,
        Target target)
    {
        #region ExtractMultipleValues
        string script = """
            (
                title: document.title,
                url: window.location.href,
                linkCount: document.querySelectorAll('a').length,
                ready: document.readyState === 'complete'
            }
            """;

        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(script, target, true));

        if (result is EvaluateResultSuccess success &&
            success.Result is KeyValuePairCollectionRemoteValue dictionaryValue)
        {
            RemoteValueDictionary data = dictionaryValue.Value;

            string title = data["title"].ConvertTo<StringRemoteValue>().Value;
            string url = data["url"].ConvertTo<StringRemoteValue>().Value;
            long linkCount = data["linkCount"].ConvertTo<NumberRemoteValue>();
            bool ready = data["ready"].ConvertTo<BooleanRemoteValue>().Value;
        }
        #endregion
    }

    /// <summary>
    /// Element collection - querySelectorAll links.
    /// </summary>
    public static async Task ElementCollection(
        BiDiDriver driver,
        Target target)
    {
        #region ElementCollection
        string script = "Array.from(document.querySelectorAll('a')).map(a => a.href)";

        EvaluateResult result = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(script, target, true));

        if (result is EvaluateResultSuccess success)
        {
            RemoteValueList links = success.Result.ConvertTo<CollectionRemoteValue>().Value;

            foreach (RemoteValue link in links)
            {
                Console.WriteLine(link.ConvertTo<StringRemoteValue>().Value);
            }
        }
        #endregion
    }

    /// <summary>
    /// Round-trip element - get element, call function for properties.
    /// </summary>
    public static async Task RoundTripElement(
        BiDiDriver driver,
        Target target)
    {
        #region Round-tripElement
        // Get element
        EvaluateResult getResult = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "document.querySelector('.target')",
                target,
                true));

        ((EvaluateResultSuccess)getResult).Result.TryConvertTo(out NodeRemoteValue? element);

        // Get properties from element
        CallFunctionCommandParameters propsParams = new CallFunctionCommandParameters(
            """
            (element) => (
                tag: element.tagName,
                text: element.textContent,
                visible: element.offsetParent !== null
            }
            """,
            target,
            false);
        propsParams.Arguments.Add(element.ToSharedReference());

        EvaluateResult propsResult = await driver.Script.CallFunctionAsync(propsParams);
        RemoteValueDictionary props = ((EvaluateResultSuccess)propsResult).Result
            .ConvertTo<KeyValuePairCollectionRemoteValue>().Value;
        #endregion
    }
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CS0168 // Variable declared but never used
#pragma warning restore CS0219 // Variable assigned but never used
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
