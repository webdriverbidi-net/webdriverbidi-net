# Remote Values

A script's result, an event's argument, or a node from `LocateNodesAsync` arrives as a `RemoteValue`, a typed tree of JavaScript values.

## Converting

- `value.As<T>()` returns the value as the `RemoteValue` type `T`, or throws `WebDriverBiDiException` if it is another type.
- `value.TryAs<T>(out T? result)` tests without throwing.
- `value.Type` is the value's `RemoteValueType`, such as `String`, `Number`, or `Node`.

Common types: `StringRemoteValue`, `NumberRemoteValue` (a `double`), `BigIntegerRemoteValue`, `BooleanRemoteValue`, `NullRemoteValue`, `UndefinedRemoteValue`, `DateRemoteValue`, `RegExpRemoteValue`, `NodeRemoteValue` (with `SharedId`), `CollectionRemoteValue` (arrays, sets, node lists; `Value` is a list), `KeyValuePairCollectionRemoteValue` (objects and maps; `Value` is a `RemoteValueDictionary` of their entries), and object references for functions, promises, and other objects.

## Script Results

`Script.EvaluateAsync` and `Script.CallFunctionAsync` return an `EvaluateResult`: `EvaluateResultSuccess` with `Result`, or `EvaluateResultException` with `ExceptionDetails` (its `Text`, line, column, and stack trace). A script that throws does **not** throw in .NET; check the result's type. With `awaitPromise: true`, a returned promise is awaited.

The WebDriverBiDi.Extensions method `driver.Script.CallFunctionAsync<T>(contextId, functionDeclaration)` returns the result as the `RemoteValue` type `T` and throws `ScriptException` when the script throws.

## Passing Values In

Arguments are `LocalValue`s: `LocalValue.String`, `LocalValue.Number`, `LocalValue.Boolean`, `LocalValue.Array`, `LocalValue.Object`, `LocalValue.Null`, `LocalValue.Undefined`, and so on. Pass a node back with `nodeRemoteValue.ToSharedReference()`.

## Ownership

`resultOwnership: Root` keeps a handle to the result in the browser, which must be released with `Script.DisownAsync`. Without it, objects are serialized by value, to the depth `SerializationOptions` allows. More: https://webdriverbidi-net.github.io/webdriverbidi-net/articles/remote-values.html
