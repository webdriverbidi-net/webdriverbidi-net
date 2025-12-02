# Storage Module

The Storage module provides functionality for managing cookies, local storage, and session storage.

## Overview

The Storage module allows you to:

- Get, set, and delete cookies
- Access local storage
- Access session storage
- Clear storage data

## Accessing the Module

```csharp
StorageModule storage = driver.Storage;
```

## Working with Cookies

### Get All Cookies

```csharp
GetCookiesCommandParameters params = new GetCookiesCommandParameters();
params.BrowsingContexts.Add(contextId);

GetCookiesCommandResult result = await driver.Storage.GetCookiesAsync(params);

foreach (var cookie in result.Cookies)
{
    Console.WriteLine($"Name: {cookie.Name}");
    Console.WriteLine($"Value: {cookie.Value.Value}");
    Console.WriteLine($"Domain: {cookie.Domain}");
    Console.WriteLine($"Path: {cookie.Path}");
    Console.WriteLine($"Secure: {cookie.Secure}");
    Console.WriteLine($"HttpOnly: {cookie.HttpOnly}");
    Console.WriteLine($"SameSite: {cookie.SameSite}");
}
```

### Get Cookies by Filter

```csharp
GetCookiesCommandParameters params = new GetCookiesCommandParameters();
params.BrowsingContexts.Add(contextId);

// Filter by name
params.Filter = new CookieFilter
{
    Name = "sessionId"
};

GetCookiesCommandResult result = await driver.Storage.GetCookiesAsync(params);
```

### Set Cookie

```csharp
PartialCookie cookie = new PartialCookie(
    "sessionId", 
    new BytesValue(BytesValueType.String, "abc123"))
{
    Domain = "example.com",
    Path = "/",
    Secure = true,
    HttpOnly = true,
    SameSite = SameSite.Strict
};

SetCookieCommandParameters params = new SetCookieCommandParameters(cookie);
await driver.Storage.SetCookieAsync(params);
```

### Set Cookie with Expiry

```csharp
PartialCookie cookie = new PartialCookie(
    "rememberMe",
    new BytesValue(BytesValueType.String, "true"))
{
    Domain = "example.com",
    Path = "/",
    Expiry = DateTimeOffset.Now.AddDays(30).ToUnixTimeSeconds()
};

SetCookieCommandParameters params = new SetCookieCommandParameters(cookie);
await driver.Storage.SetCookieAsync(params);
```

### Delete Cookie

```csharp
CookieFilter filter = new CookieFilter
{
    Name = "sessionId",
    Domain = "example.com"
};

DeleteCookiesCommandParameters params = new DeleteCookiesCommandParameters();
params.Filter = filter;

await driver.Storage.DeleteCookiesAsync(params);
```

### Delete All Cookies

```csharp
DeleteCookiesCommandParameters params = new DeleteCookiesCommandParameters();
params.BrowsingContexts.Add(contextId);

await driver.Storage.DeleteCookiesAsync(params);
```

## Working with Storage

### Get Local Storage Keys

```csharp
// Navigate first to set storage origin
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com"));

GetStorageKeyCommandParameters params = new GetStorageKeyCommandParameters();
params.BrowsingContexts.Add(contextId);

GetStorageKeyCommandResult result = await driver.Storage.GetStorageKeyAsync(params);
```

### Delete Storage

```csharp
// Delete local and session storage
DeleteStorageCommandParameters params = new DeleteStorageCommandParameters();
params.BrowsingContexts.Add(contextId);

await driver.Storage.DeleteStorageAsync(params);
```

## Common Patterns

### Save and Restore Session

```csharp
// Save cookies
GetCookiesCommandResult savedCookies = await driver.Storage.GetCookiesAsync(
    new GetCookiesCommandParameters { BrowsingContexts = { contextId } });

// ... later, restore cookies
foreach (var cookie in savedCookies.Cookies)
{
    PartialCookie newCookie = new PartialCookie(cookie.Name, cookie.Value)
    {
        Domain = cookie.Domain,
        Path = cookie.Path,
        Secure = cookie.Secure,
        HttpOnly = cookie.HttpOnly,
        SameSite = cookie.SameSite,
        Expiry = cookie.Expiry
    };
    
    await driver.Storage.SetCookieAsync(
        new SetCookieCommandParameters(newCookie));
}
```

### Clean State Between Tests

```csharp
// Clear all cookies
await driver.Storage.DeleteCookiesAsync(
    new DeleteCookiesCommandParameters { BrowsingContexts = { contextId } });

// Clear storage
await driver.Storage.DeleteStorageAsync(
    new DeleteStorageCommandParameters { BrowsingContexts = { contextId } });
```

### Set Authentication Cookie

```csharp
PartialCookie authCookie = new PartialCookie(
    "authToken",
    new BytesValue(BytesValueType.String, "your-auth-token"))
{
    Domain = "example.com",
    Path = "/",
    Secure = true,
    HttpOnly = true,
    SameSite = SameSite.Strict
};

SetCookieCommandParameters params = new SetCookieCommandParameters(authCookie);
await driver.Storage.SetCookieAsync(params);

// Now navigate - cookie will be sent
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com/dashboard"));
```

## Best Practices

1. **Set cookies before navigation**: Set cookies before navigating to the domain
2. **Match domains correctly**: Cookie domain must match the current page domain
3. **Use appropriate SameSite**: Choose `Strict`, `Lax`, or `None` based on needs
4. **Clean up between tests**: Clear cookies and storage for test isolation
5. **Handle secure cookies**: Set `Secure` flag for HTTPS-only cookies

## Next Steps

- [Browser Module](browser.md): Managing user contexts
- [Network Module](network.md): Monitoring cookie-related network traffic
- [API Reference](../../api/index.md): Complete API documentation

