# Script Module

The Script module provides functionality for executing JavaScript in the browser, managing preload scripts, and working with JavaScript values and objects.

## Overview

The Script module allows you to:

- Execute JavaScript code in browsing contexts
- Call JavaScript functions with arguments
- Add preload scripts that run before page scripts
- Manage script execution realms (sandboxes)
- Send messages from scripts back to your code
- Work with JavaScript objects and DOM elements

## Accessing the Module

```csharp
ScriptModule script = driver.Script;
```

## Evaluating JavaScript

### Evaluate Expression

```csharp
EvaluateCommandParameters params = new EvaluateCommandParameters(
    "document.title",
    new ContextTarget(contextId),
    true  // Await promises
);

EvaluateResult result = await driver.Script.EvaluateAsync(params);

if (result is EvaluateResultSuccess success)
{
    string title = success.Result.ValueAs<string>();
    Console.WriteLine($"Title: {title}");
}
```

### Evaluate with Complex Expression

```csharp
string expression = @"
{
    title: document.title,
    url: window.location.href,
    elementCount: document.querySelectorAll('*').length
}";

EvaluateCommandParameters params = new EvaluateCommandParameters(
    expression,
    new ContextTarget(contextId),
    true
);

EvaluateResult result = await driver.Script.EvaluateAsync(params);
```

## Calling Functions

### Call Function with Arguments

```csharp
string functionDefinition = "(a, b) => a + b";

CallFunctionCommandParameters params = new CallFunctionCommandParameters(
    functionDefinition,
    new ContextTarget(contextId),
    true
);
params.Arguments.Add(LocalValue.Number(5));
params.Arguments.Add(LocalValue.Number(10));

EvaluateResult result = await driver.Script.CallFunctionAsync(params);

if (result is EvaluateResultSuccess success)
{
    long sum = success.Result.ValueAs<long>();
    Console.WriteLine($"Sum: {sum}");  // 15
}
```

### Call Function with DOM Element

```csharp
// First, get an element
EvaluateResult elementResult = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        "document.querySelector('button')",
        new ContextTarget(contextId),
        true
    )
);

if (elementResult is EvaluateResultSuccess elementSuccess)
{
    RemoteValue element = elementSuccess.Result;
    
    // Now call a function with that element
    string functionDefinition = "(element) => element.click()";
    
    CallFunctionCommandParameters params = new CallFunctionCommandParameters(
        functionDefinition,
        new ContextTarget(contextId),
        false
    );
    params.Arguments.Add(element.ToSharedReference());
    
    await driver.Script.CallFunctionAsync(params);
}
```

### Call Method on Object

```csharp
// Get object reference
EvaluateResult objectResult = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        "document.getElementById('myDiv')",
        new ContextTarget(contextId),
        true
    )
);

if (objectResult is EvaluateResultSuccess objectSuccess)
{
    RemoteValue divElement = objectSuccess.Result;
    
    // Call getAttribute method
    string functionDefinition = "(element, attrName) => element.getAttribute(attrName)";
    
    CallFunctionCommandParameters params = new CallFunctionCommandParameters(
        functionDefinition,
        new ContextTarget(contextId),
        false
    );
    params.Arguments.Add(divElement.ToSharedReference());
    params.Arguments.Add(LocalValue.String("class"));
    
    EvaluateResult result = await driver.Script.CallFunctionAsync(params);
    if (result is EvaluateResultSuccess success)
    {
        string className = success.Result.ValueAs<string>();
        Console.WriteLine($"Class: {className}");
    }
}
```

## Execution Targets

Scripts can be executed in different contexts:

### Context Target

Execute in a specific browsing context:

```csharp
ContextTarget target = new ContextTarget(contextId);

EvaluateCommandParameters params = new EvaluateCommandParameters(
    "document.title",
    target,
    true
);
```

### Realm Target

Execute in a specific execution realm:

```csharp
RealmTarget target = new RealmTarget(realmId);

EvaluateCommandParameters params = new EvaluateCommandParameters(
    "window.myCustomProperty",
    target,
    true
);
```

### Sandboxed Execution

Execute in a sandbox to isolate from page scripts:

```csharp
ContextTarget target = new ContextTarget(contextId)
{
    Sandbox = "myIsolatedSandbox"
};

EvaluateCommandParameters params = new EvaluateCommandParameters(
    "window.isolatedData = { value: 42 }",
    target,
    false
);
await driver.Script.EvaluateAsync(params);

// Later, access the same sandbox
target = new ContextTarget(contextId)
{
    Sandbox = "myIsolatedSandbox"
};
params = new EvaluateCommandParameters(
    "window.isolatedData.value",
    target,
    true
);
EvaluateResult result = await driver.Script.EvaluateAsync(params);
```

## Preload Scripts

Preload scripts run before any page scripts, allowing you to:

- Inject utilities into every page
- Monitor page behavior
- Modify page behavior before it starts

### Add Preload Script

```csharp
string preloadScript = @"
() => {
    window.myUtility = {
        getElementTag: (element) => element.tagName
    };
}";

AddPreloadScriptCommandParameters params = 
    new AddPreloadScriptCommandParameters(preloadScript);

AddPreloadScriptCommandResult result = 
    await driver.Script.AddPreloadScriptAsync(params);

string preloadScriptId = result.PreloadScriptId;
```

### Preload Script with Arguments

```csharp
string preloadScript = @"
(config) => {
    window.myConfig = config;
    console.log('Config loaded:', config.appName);
}";

AddPreloadScriptCommandParameters params = 
    new AddPreloadScriptCommandParameters(preloadScript);

params.Arguments.Add(LocalValue.Object(new Dictionary<string, LocalValue>
{
    { "appName", LocalValue.String("MyApp") },
    { "version", LocalValue.Number(1) }
}));

await driver.Script.AddPreloadScriptAsync(params);
```

### Preload Script with Channel

Use channels to send messages from preload scripts:

```csharp
// Subscribe to script messages
SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.Script.OnMessage.EventName);
await driver.Session.SubscribeAsync(subscribe);

// Handle messages
driver.Script.OnMessage.AddObserver((MessageEventArgs e) =>
{
    if (e.ChannelId == "myChannel")
    {
        Console.WriteLine($"Message from preload: {e.Data.ValueAs<string>()}");
    }
});

// Add preload script with channel
string preloadScript = @"
(channel) => {
    window.addEventListener('load', () => {
        channel('Page loaded');
    });
}";

ChannelValue channel = new ChannelValue(
    new ChannelProperties("myChannel"));

AddPreloadScriptCommandParameters params = 
    new AddPreloadScriptCommandParameters(preloadScript);
params.Arguments.Add(channel);

await driver.Script.AddPreloadScriptAsync(params);
```

### Sandboxed Preload Script

```csharp
string preloadScript = @"
() => {
    window.isolatedUtils = {
        getPageTitle: () => document.title
    };
}";

AddPreloadScriptCommandParameters params = 
    new AddPreloadScriptCommandParameters(preloadScript)
    {
        Sandbox = "utilsSandbox"
    };

await driver.Script.AddPreloadScriptAsync(params);

// Later, call the utility from the same sandbox
ContextTarget target = new ContextTarget(contextId)
{
    Sandbox = "utilsSandbox"
};

EvaluateCommandParameters evalParams = new EvaluateCommandParameters(
    "window.isolatedUtils.getPageTitle()",
    target,
    true
);
EvaluateResult result = await driver.Script.EvaluateAsync(evalParams);
```

### Remove Preload Script

```csharp
RemovePreloadScriptCommandParameters params = 
    new RemovePreloadScriptCommandParameters(preloadScriptId);

await driver.Script.RemovePreloadScriptAsync(params);
```

## Working with Remote Values

### Accessing Primitive Values

```csharp
EvaluateResult result = await driver.Script.EvaluateAsync(params);

if (result is EvaluateResultSuccess success)
{
    RemoteValue value = success.Result;
    
    switch (value.Type)
    {
        case "string":
            string str = value.ValueAs<string>();
            break;
        case "number":
            long num = value.ValueAs<long>();
            // or double doubleNum = value.ValueAs<double>();
            break;
        case "boolean":
            bool flag = value.ValueAs<bool>();
            break;
        case "null":
        case "undefined":
            // Value is null/undefined
            break;
    }
}
```

### Accessing Objects

```csharp
string expression = @"
({
    name: 'John',
    age: 30,
    active: true
})";

EvaluateCommandParameters params = new EvaluateCommandParameters(
    expression,
    new ContextTarget(contextId),
    true
);

EvaluateResult result = await driver.Script.EvaluateAsync(params);

if (result is EvaluateResultSuccess success)
{
    RemoteValue obj = success.Result;
    
    // Access as dictionary
    var dict = obj.ValueAs<Dictionary<string, object>>();
    Console.WriteLine($"Name: {dict["name"]}");
    Console.WriteLine($"Age: {dict["age"]}");
}
```

### Accessing Arrays

```csharp
string expression = "[1, 2, 3, 4, 5]";

EvaluateCommandParameters params = new EvaluateCommandParameters(
    expression,
    new ContextTarget(contextId),
    true
);

EvaluateResult result = await driver.Script.EvaluateAsync(params);

if (result is EvaluateResultSuccess success)
{
    RemoteValue array = success.Result;
    
    // Access as list
    var list = array.ValueAs<List<object>>();
    Console.WriteLine($"Array length: {list.Count}");
    foreach (var item in list)
    {
        Console.WriteLine($"  Item: {item}");
    }
}
```

### Working with DOM Elements

```csharp
EvaluateCommandParameters params = new EvaluateCommandParameters(
    "document.querySelector('button')",
    new ContextTarget(contextId),
    true
);

EvaluateResult result = await driver.Script.EvaluateAsync(params);

if (result is EvaluateResultSuccess success)
{
    RemoteValue element = success.Result;
    
    // Check if it's a node
    if (element.Type == "node")
    {
        // Get node properties
        NodeProperties? nodeProps = element.ValueAs<NodeProperties>();
        
        if (nodeProps != null)
        {
            Console.WriteLine($"Tag: {nodeProps.LocalName}");
            Console.WriteLine($"Node type: {nodeProps.NodeType}");
            
            // Get attributes
            if (nodeProps.Attributes != null)
            {
                foreach (var attr in nodeProps.Attributes)
                {
                    Console.WriteLine($"  {attr.Key} = {attr.Value}");
                }
            }
        }
        
        // Get shared ID for later use
        string? sharedId = element.SharedId;
        
        // Create reference to pass to other commands
        SharedReference elementRef = element.ToSharedReference();
    }
}
```

## Creating Local Values

When passing values to JavaScript:

### Primitive Values

```csharp
CallFunctionCommandParameters params = new CallFunctionCommandParameters(
    "(str, num, bool) => ({ str, num, bool })",
    new ContextTarget(contextId),
    true
);

params.Arguments.Add(LocalValue.String("Hello"));
params.Arguments.Add(LocalValue.Number(42));
params.Arguments.Add(LocalValue.Boolean(true));

await driver.Script.CallFunctionAsync(params);
```

### Special Values

```csharp
params.Arguments.Add(LocalValue.Null());
params.Arguments.Add(LocalValue.Undefined());
params.Arguments.Add(LocalValue.Number(double.PositiveInfinity));
params.Arguments.Add(LocalValue.Number(double.NaN));
```

### Objects

```csharp
Dictionary<string, LocalValue> obj = new Dictionary<string, LocalValue>
{
    { "name", LocalValue.String("John") },
    { "age", LocalValue.Number(30) },
    { "active", LocalValue.Boolean(true) }
};

params.Arguments.Add(LocalValue.Object(obj));
```

### Arrays

```csharp
List<LocalValue> array = new List<LocalValue>
{
    LocalValue.Number(1),
    LocalValue.Number(2),
    LocalValue.Number(3)
};

params.Arguments.Add(LocalValue.Array(array));
```

### Dates

```csharp
params.Arguments.Add(LocalValue.Date(DateTime.Now));
```

### Regular Expressions

```csharp
params.Arguments.Add(LocalValue.RegularExpression("\\d+", "g"));
```

## Handling Script Errors

### Try-Catch Pattern

```csharp
EvaluateCommandParameters params = new EvaluateCommandParameters(
    "throw new Error('Something went wrong')",
    new ContextTarget(contextId),
    true
);

EvaluateResult result = await driver.Script.EvaluateAsync(params);

if (result is EvaluateResultException exception)
{
    Console.WriteLine($"Error: {exception.ExceptionDetails.Text}");
    Console.WriteLine($"Line: {exception.ExceptionDetails.LineNumber}");
    Console.WriteLine($"Column: {exception.ExceptionDetails.ColumnNumber}");
    
    if (exception.ExceptionDetails.StackTrace != null)
    {
        Console.WriteLine($"Stack: {exception.ExceptionDetails.StackTrace.CallFrames.Count} frames");
    }
}
```

### Catching JavaScript Errors

```csharp
string safeExpression = @"
try {
    return dangerousOperation();
} catch (error) {
    return { error: error.message };
}";

EvaluateCommandParameters params = new EvaluateCommandParameters(
    safeExpression,
    new ContextTarget(contextId),
    true
);

EvaluateResult result = await driver.Script.EvaluateAsync(params);
// Will always be success, check for error property in result
```

## Awaiting Promises

### Automatic Promise Resolution

```csharp
// Fetch API returns a promise
string expression = @"
fetch('https://api.example.com/data')
    .then(response => response.json())
";

EvaluateCommandParameters params = new EvaluateCommandParameters(
    expression,
    new ContextTarget(contextId),
    true  // awaitPromise = true
);

EvaluateResult result = await driver.Script.EvaluateAsync(params);
// Result will be the resolved JSON data
```

### Async Functions

```csharp
string functionDefinition = @"
async () => {
    const response = await fetch('/api/data');
    const data = await response.json();
    return data;
}";

CallFunctionCommandParameters params = new CallFunctionCommandParameters(
    functionDefinition,
    new ContextTarget(contextId),
    true  // awaitPromise = true
);

EvaluateResult result = await driver.Script.CallFunctionAsync(params);
```

## Events

### Script Message Event

```csharp
driver.Script.OnMessage.AddObserver((MessageEventArgs e) =>
{
    Console.WriteLine($"Channel: {e.ChannelId}");
    Console.WriteLine($"Data: {e.Data.ValueAs<string>()}");
    Console.WriteLine($"Source context: {e.Source.Context}");
});

SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.Script.OnMessage.EventName);
await driver.Session.SubscribeAsync(subscribe);
```

### Realm Created/Destroyed

```csharp
driver.Script.OnRealmCreated.AddObserver((RealmCreatedEventArgs e) =>
{
    Console.WriteLine($"Realm created: {e.RealmId}");
    Console.WriteLine($"Type: {e.Type}");
});

driver.Script.OnRealmDestroyed.AddObserver((RealmDestroyedEventArgs e) =>
{
    Console.WriteLine($"Realm destroyed: {e.RealmId}");
});
```

## Common Patterns

### Element Interaction Pattern

```csharp
// Find element
LocateNodesCommandResult locateResult = await driver.BrowsingContext.LocateNodesAsync(
    new LocateNodesCommandParameters(contextId, new CssLocator("button")));

RemoteValue element = locateResult.Nodes[0];

// Click element
CallFunctionCommandParameters clickParams = new CallFunctionCommandParameters(
    "(element) => element.click()",
    new ContextTarget(contextId),
    false
);
clickParams.Arguments.Add(element.ToSharedReference());
await driver.Script.CallFunctionAsync(clickParams);
```

### Get Element Properties Pattern

```csharp
// Get multiple properties at once
string functionDefinition = @"
(element) => ({
    tag: element.tagName,
    text: element.textContent,
    visible: element.offsetParent !== null,
    enabled: !element.disabled,
    value: element.value
})";

CallFunctionCommandParameters params = new CallFunctionCommandParameters(
    functionDefinition,
    new ContextTarget(contextId),
    false
);
params.Arguments.Add(elementReference);

EvaluateResult result = await driver.Script.CallFunctionAsync(params);
```

### Wait for Condition Pattern

```csharp
string preloadScript = @"
(channel) => {
    const checkCondition = () => {
        if (document.querySelector('.loaded')) {
            channel({ loaded: true });
        } else {
            setTimeout(checkCondition, 100);
        }
    };
    checkCondition();
}";

TaskCompletionSource<bool> conditionMet = new TaskCompletionSource<bool>();

driver.Script.OnMessage.AddObserver((MessageEventArgs e) =>
{
    if (e.ChannelId == "conditionChannel")
    {
        conditionMet.SetResult(true);
    }
});

ChannelValue channel = new ChannelValue(
    new ChannelProperties("conditionChannel"));

AddPreloadScriptCommandParameters params = 
    new AddPreloadScriptCommandParameters(preloadScript);
params.Arguments.Add(channel);

await driver.Script.AddPreloadScriptAsync(params);
await driver.BrowsingContext.NavigateAsync(navParams);

await conditionMet.Task;
Console.WriteLine("Condition met!");
```

## Best Practices

1. **Use `awaitPromise`**: Set to `true` for async operations
2. **Handle exceptions**: Check for `EvaluateResultException`
3. **Use sandboxes**: Isolate your scripts from page scripts
4. **Cache element references**: Store `SharedReference` for reuse
5. **Prefer functions over eval**: Use `CallFunctionAsync` for better isolation
6. **Remove preload scripts**: Clean up when no longer needed

## Next Steps

- [Remote Values Guide](../remote-values.md): Deep dive into JavaScript value handling
- [Browsing Context Module](browsing-context.md): Finding elements
- [Preload Scripts Example](../examples/preload-scripts.md): Complete examples
- [API Reference](../../api/index.md): Complete API documentation

## API Reference

See the [API documentation](../../api/index.md) for complete details on all classes and methods in the Script module.

